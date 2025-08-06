import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TodoItem, TodoPriority } from '../../models/todo.model';
import { TodoService } from '../../services/todo.service';
import { TodoDialogComponent } from '../todo-dialog/todo-dialog.component';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-todo-list',
  templateUrl: './todo-list.component.html',
  styleUrls: ['./todo-list.component.scss']
})
export class TodoListComponent implements OnInit {
  todos: TodoItem[] = [];
  loading = false;
  displayedColumns: string[] = ['title', 'priority', 'dueDate', 'status', 'actions'];

  constructor(
    private todoService: TodoService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.loadTodos();
  }

  loadTodos(): void {
    this.loading = true;
    this.todoService.getAllTodos().subscribe({
      next: (todos) => {
        this.todos = todos.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
        this.loading = false;
      },
      error: (error) => {
        this.snackBar.open('Error loading todos', 'Close', { duration: 3000 });
        this.loading = false;
        console.error('Error loading todos:', error);
      }
    });
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(TodoDialogComponent, {
      width: '500px',
      data: { mode: 'create' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.todoService.createTodo(result).subscribe({
          next: () => {
            this.snackBar.open('Todo created successfully', 'Close', { duration: 3000 });
            this.loadTodos();
          },
          error: (error) => {
            this.snackBar.open('Error creating todo', 'Close', { duration: 3000 });
            console.error('Error creating todo:', error);
          }
        });
      }
    });
  }

  openEditDialog(todo: TodoItem): void {
    const dialogRef = this.dialog.open(TodoDialogComponent, {
      width: '500px',
      data: { mode: 'edit', todo: { ...todo } }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.todoService.updateTodo(todo.id, result).subscribe({
          next: () => {
            this.snackBar.open('Todo updated successfully', 'Close', { duration: 3000 });
            this.loadTodos();
          },
          error: (error) => {
            this.snackBar.open('Error updating todo', 'Close', { duration: 3000 });
            console.error('Error updating todo:', error);
          }
        });
      }
    });
  }

  deleteTodo(todo: TodoItem): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Confirm Delete',
        message: `Are you sure you want to delete "${todo.title}"?`,
        confirmText: 'Delete',
        cancelText: 'Cancel'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.todoService.deleteTodo(todo.id).subscribe({
          next: () => {
            this.snackBar.open('Todo deleted successfully', 'Close', { duration: 3000 });
            this.loadTodos();
          },
          error: (error) => {
            this.snackBar.open('Error deleting todo', 'Close', { duration: 3000 });
            console.error('Error deleting todo:', error);
          }
        });
      }
    });
  }

  toggleComplete(todo: TodoItem): void {
    if (!todo.isCompleted) {
      this.todoService.completeTodo(todo.id).subscribe({
        next: () => {
          this.snackBar.open('Todo marked as completed', 'Close', { duration: 3000 });
          this.loadTodos();
        },
        error: (error) => {
          this.snackBar.open('Error updating todo', 'Close', { duration: 3000 });
          console.error('Error updating todo:', error);
        }
      });
    } else {
      // Toggle back to incomplete
      const updateData = {
        title: todo.title,
        description: todo.description,
        isCompleted: false,
        dueDate: todo.dueDate,
        priority: todo.priority,
        tags: todo.tags
      };

      this.todoService.updateTodo(todo.id, updateData).subscribe({
        next: () => {
          this.snackBar.open('Todo marked as incomplete', 'Close', { duration: 3000 });
          this.loadTodos();
        },
        error: (error) => {
          this.snackBar.open('Error updating todo', 'Close', { duration: 3000 });
          console.error('Error updating todo:', error);
        }
      });
    }
  }

  getPriorityColor(priority: TodoPriority): string {
    switch (priority) {
      case TodoPriority.Low: return 'primary';
      case TodoPriority.Medium: return 'accent';
      case TodoPriority.High: return 'warn';
      case TodoPriority.Urgent: return 'warn';
      default: return 'primary';
    }
  }

  getPriorityText(priority: TodoPriority): string {
    return TodoPriority[priority];
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return '';
    return new Date(date).toLocaleDateString();
  }
} 