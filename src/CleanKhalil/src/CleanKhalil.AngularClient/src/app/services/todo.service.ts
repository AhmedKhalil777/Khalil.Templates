import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TodoItem, CreateTodoItem, UpdateTodoItem } from '../models/todo.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class TodoService {
  private apiUrl = `${environment.apiUrl}/todo`;

  constructor(private http: HttpClient) { }

  getAllTodos(): Observable<TodoItem[]> {
    return this.http.get<TodoItem[]>(this.apiUrl);
  }

  getTodoById(id: number): Observable<TodoItem> {
    return this.http.get<TodoItem>(`${this.apiUrl}/${id}`);
  }

  createTodo(todo: CreateTodoItem): Observable<TodoItem> {
    return this.http.post<TodoItem>(this.apiUrl, todo);
  }

  updateTodo(id: number, todo: UpdateTodoItem): Observable<TodoItem> {
    return this.http.put<TodoItem>(`${this.apiUrl}/${id}`, todo);
  }

  deleteTodo(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  completeTodo(id: number): Observable<TodoItem> {
    return this.http.patch<TodoItem>(`${this.apiUrl}/${id}/complete`, {});
  }

  getCompletedTodos(): Observable<TodoItem[]> {
    return this.http.get<TodoItem[]>(`${this.apiUrl}/completed`);
  }

  getPendingTodos(): Observable<TodoItem[]> {
    return this.http.get<TodoItem[]>(`${this.apiUrl}/pending`);
  }
} 