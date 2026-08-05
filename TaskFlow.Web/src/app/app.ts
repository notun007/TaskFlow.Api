import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { TaskApiService } from './task-api.service';

type Task = { id: string; title: string; project: string; software: string; owner: string; status: string; priority: string; due: string; severity: string };

@Component({
  selector: 'app-root',
  standalone: true,
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  private readonly taskApi = inject(TaskApiService);
  readonly activeNav = signal('Dashboard');
  readonly search = signal('');
  readonly showCreate = signal(false);
  readonly tasks = signal<Task[]>([
    { id: 'TF-2408', title: 'Duplicate transaction displayed after fund transfer', project: 'Internet Banking Upgrade', software: 'Internet Banking', owner: 'Nadia Rahman', status: 'In Progress', priority: 'Critical', due: 'Today', severity: 'Critical' },
    { id: 'TF-2404', title: 'Validate regulatory report field mappings', project: 'Regulatory Reporting', software: 'Core Banking', owner: 'Arif Chowdhury', status: 'Ready for Testing', priority: 'High', due: 'Aug 06', severity: 'High' },
    { id: 'TF-2398', title: 'Confirm vendor SLA escalation contacts', project: 'Payment Gateway Support', software: 'Vendor Payment Gateway', owner: 'Sarah Ahmed', status: 'Pending Vendor', priority: 'Medium', due: 'Aug 07', severity: 'Medium' },
    { id: 'TF-2389', title: 'UAT evidence for mobile login enhancement', project: 'Mobile Banking Upgrade', software: 'Mobile Banking', owner: 'Tanvir Hasan', status: 'UAT', priority: 'High', due: 'Aug 08', severity: 'High' },
    { id: 'TF-2381', title: 'Update disaster recovery runbook', project: 'Core Banking Upgrade', software: 'Core Banking', owner: 'Maliha Sultana', status: 'Submitted', priority: 'Low', due: 'Aug 12', severity: 'Low' }
  ]);
  readonly filteredTasks = computed(() => {
    const term = this.search().toLowerCase();
    return this.tasks().filter(task => !term || `${task.id} ${task.title} ${task.project} ${task.owner}`.toLowerCase().includes(term));
  });

  ngOnInit() {
    this.taskApi.getTasks().subscribe({
      next: apiTasks => {
        if (apiTasks.length) {
          this.tasks.set(apiTasks.map(task => ({
            id: task.taskNumber,
            title: task.title,
            project: task.projectName ?? 'Unassigned project',
            software: task.type,
            owner: 'Unassigned',
            status: task.status,
            priority: task.priority,
            due: task.dueDate ? new Date(task.dueDate).toLocaleDateString() : 'No due date',
            severity: task.priority
          })));
        }
      },
      error: () => undefined
    });
  }

  setNav(item: string) { this.activeNav.set(item); }
  initials(name: string) { return name.split(' ').map(part => part[0]).join(''); }
  createTask() { this.showCreate.set(false); this.activeNav.set('My Work'); }
}
