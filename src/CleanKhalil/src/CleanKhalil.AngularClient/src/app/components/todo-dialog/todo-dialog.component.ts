import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { TodoItem, CreateTodoItem, UpdateTodoItem, TodoPriority } from '../../models/todo.model';

export interface TodoDialogData {
  mode: 'create' | 'edit';
  todo?: TodoItem;
}

@Component({
  selector: 'app-todo-dialog',
  templateUrl: './todo-dialog.component.html',
  styleUrls: ['./todo-dialog.component.scss']
})
export class TodoDialogComponent implements OnInit {
  todoForm!: FormGroup;
  isEditMode: boolean;
  priorities = [
    { value: TodoPriority.Low, label: 'Low' },
    { value: TodoPriority.Medium, label: 'Medium' },
    { value: TodoPriority.High, label: 'High' },
    { value: TodoPriority.Urgent, label: 'Urgent' }
  ];

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<TodoDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: TodoDialogData
  ) {
    this.isEditMode = data.mode === 'edit';
  }

  ngOnInit(): void {
    this.initForm();
  }

  private initForm(): void {
    const todo = this.data.todo;
    
    this.todoForm = this.fb.group({
      title: [todo?.title || '', [Validators.required, Validators.minLength(1)]],
      description: [todo?.description || ''],
      priority: [todo?.priority || TodoPriority.Medium, Validators.required],
      dueDate: [todo?.dueDate ? new Date(todo.dueDate) : null],
      tags: [todo?.tags || ''],
      isCompleted: [todo?.isCompleted || false]
    });
  }

  onSubmit(): void {
    if (this.todoForm.valid) {
      const formValue = this.todoForm.value;
      
      if (this.isEditMode) {
        const updateData: UpdateTodoItem = {
          title: formValue.title,
          description: formValue.description,
          isCompleted: formValue.isCompleted,
          dueDate: formValue.dueDate,
          priority: formValue.priority,
          tags: formValue.tags
        };
        this.dialogRef.close(updateData);
      } else {
        const createData: CreateTodoItem = {
          title: formValue.title,
          description: formValue.description,
          dueDate: formValue.dueDate,
          priority: formValue.priority,
          tags: formValue.tags
        };
        this.dialogRef.close(createData);
      }
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  getDialogTitle(): string {
    return this.isEditMode ? 'Edit Todo' : 'Create New Todo';
  }

  getSubmitButtonText(): string {
    return this.isEditMode ? 'Update' : 'Create';
  }
} 