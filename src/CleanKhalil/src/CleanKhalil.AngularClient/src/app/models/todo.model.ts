export enum TodoPriority {
  Low = 1,
  Medium = 2,
  High = 3,
  Urgent = 4
}

export interface TodoItem {
  id: number;
  title: string;
  description?: string;
  isCompleted: boolean;
  dueDate?: Date;
  priority: TodoPriority;
  completedAt?: Date;
  tags?: string;
  createdAt: Date;
  updatedAt?: Date;
  createdBy?: string;
  updatedBy?: string;
}

export interface CreateTodoItem {
  title: string;
  description?: string;
  dueDate?: Date;
  priority: TodoPriority;
  tags?: string;
}

export interface UpdateTodoItem {
  title: string;
  description?: string;
  isCompleted: boolean;
  dueDate?: Date;
  priority: TodoPriority;
  tags?: string;
} 