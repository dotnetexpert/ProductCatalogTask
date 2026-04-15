import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';

export type TableSortDirection = 'asc' | 'desc';

export type TableColumn<T> =
  | {
      id: string;
      header: string;
      type?: 'text';
      value: (row: T) => unknown;
      sortable?: boolean;
      searchable?: boolean;
      align?: 'left' | 'center' | 'right';
      width?: string;
    }
  | {
      id: string;
      header: string;
      type: 'actions';
      actions: Array<{
        id: string;
        label: string;
        variant?: 'primary' | 'ghost' | 'danger';
      }>;
      align?: 'left' | 'center' | 'right';
      width?: string;
    };

@Component({
  selector: 'app-generic-table',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './generic-table.component.html',
  styleUrl: './generic-table.component.scss',
})
export class GenericTableComponent<T> implements OnChanges {
  @Input({ required: true }) columns: Array<TableColumn<T>> = [];
  @Input({ required: true }) data: Array<T> = [];
  @Input() loading = false;
  @Input() title?: string;
  @Input() subtitle?: string;

  @Output() action = new EventEmitter<{ actionId: string; row: T }>();

  query = '';
  sortBy?: string;
  sortDir: TableSortDirection = 'asc';
  page = 1;
  pageSize = 10;
  pageSizeOptions = [5, 10, 20, 50];

  trackByColumn = (_: number, col: TableColumn<T>) => col.id;
  trackByRow = (i: number) => i;


  constructor(private readonly cdr: ChangeDetectorRef) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['columns']) console.log('Table Columns Changed:', changes['columns'].currentValue);
    if (changes['data']) console.log('Table Input Data Changed:', changes['data'].currentValue);
    if (changes['data']) console.log('Table Data Length:', (changes['data'].currentValue as Array<T> | undefined)?.length);
    this.cdr.detectChanges();
  }

  isActionsColumn(col: TableColumn<T>): col is Extract<TableColumn<T>, { type: 'actions' }> {
    return (col as any).type === 'actions';
  }

  isValueColumn(col: TableColumn<T>): col is Extract<TableColumn<T>, { value: (row: T) => unknown }> {
    return !this.isActionsColumn(col) && typeof (col as any).value === 'function';
  }

  valueText(col: TableColumn<T>, row: T): string {
    if (!this.isValueColumn(col)) return '—';
    return this.cellText(col, row);
  }

  get searchableColumns(): Array<Extract<TableColumn<T>, { value: (row: T) => unknown }>> {
    return this.columns.filter(
      (c): c is Extract<TableColumn<T>, { value: (row: T) => unknown }> =>
        (c as any).value && (c as any).type !== 'actions' && (c as any).searchable !== false,
    );
  }

  get sortableColumns(): Array<Extract<TableColumn<T>, { value: (row: T) => unknown }>> {
    return this.columns.filter(
      (c): c is Extract<TableColumn<T>, { value: (row: T) => unknown }> =>
        (c as any).value && (c as any).type !== 'actions' && (c as any).sortable !== false,
    );
  }

  get filteredData(): Array<T> {
    const q = this.query.trim().toLowerCase();
    if (!q) return this.data;

    const cols = this.searchableColumns;
    return this.data.filter(row =>
      cols.some(c => {
        const v = c.value(row);
        if (v === null || v === undefined) return false;
        return String(v).toLowerCase().includes(q);
      }),
    );
  }

  get sortedData(): Array<T> {
    const rows = [...this.filteredData];
    if (!this.sortBy) return rows;

    const col = this.columns.find(c => c.id === this.sortBy) as Extract<
      TableColumn<T>,
      { value: (row: T) => unknown }
    > | null;
    if (!col || (col as any).type === 'actions') return rows;

    const dir = this.sortDir === 'asc' ? 1 : -1;
    rows.sort((a, b) => {
      const av = col.value(a);
      const bv = col.value(b);

      if (av === null || av === undefined) return 1;
      if (bv === null || bv === undefined) return -1;

      if (typeof av === 'number' && typeof bv === 'number') return (av - bv) * dir;

      const as = String(av).toLowerCase();
      const bs = String(bv).toLowerCase();
      if (as < bs) return -1 * dir;
      if (as > bs) return 1 * dir;
      return 0;
    });
    return rows;
  }

  get total(): number {
    return this.sortedData.length;
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.total / this.pageSize));
  }

  get pageStart(): number {
    if (this.total === 0) return 0;
    return (this.page - 1) * this.pageSize + 1;
  }

  get pageEnd(): number {
    return Math.min(this.total, this.page * this.pageSize);
  }

  get pagedData(): Array<T> {
    const p = Math.min(Math.max(1, this.page), this.totalPages);
    if (p !== this.page) this.page = p;
    const start = (p - 1) * this.pageSize;
    return this.sortedData.slice(start, start + this.pageSize);
  }

  onHeaderClick(col: TableColumn<T>) {
    if ((col as any).type === 'actions') return;
    if ((col as any).sortable === false) return;

    if (this.sortBy === col.id) {
      this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
      return;
    }

    this.sortBy = col.id;
    this.sortDir = 'asc';
  }

  onQueryChange() {
    this.page = 1;
  }

  onPageSizeChange() {
    this.page = 1;
  }

  goToPage(p: number) {
    this.page = Math.min(Math.max(1, p), this.totalPages);
  }

  emitAction(actionId: string, row: T) {
    this.action.emit({ actionId, row });
  }

  cellText(col: Extract<TableColumn<T>, { value: (row: T) => unknown }>, row: T): string {
    const v = col.value(row);
    if (v === null || v === undefined) return '—';
    return String(v);
  }
}

