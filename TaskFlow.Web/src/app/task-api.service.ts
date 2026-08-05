import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export type ApiTask = {
  id: string;
  taskNumber: string;
  title: string;
  type: string;
  status: string;
  priority: string;
  dueDate?: string | null;
  projectName?: string | null;
};

@Injectable({ providedIn: 'root' })
export class TaskApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5100/api';

  getTasks(search?: string): Observable<ApiTask[]> {
    const query = search ? `?search=${encodeURIComponent(search)}` : '';
    return this.http.get<ApiTask[]>(`${this.baseUrl}/tasks${query}`);
  }
}
