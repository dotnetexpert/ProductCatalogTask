import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, ElementRef, ViewChild } from '@angular/core';
import { AfterViewInit, Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize, Subject, takeUntil } from 'rxjs';
import {
  Client,
  CreateProductCommand,
  GetProductsQuery,
  ProductDto,
  ProductDtoListGridResultApiResponse,
  UpdateProductCommand,
} from '../../../api/client';
import {
  GenericTableComponent,
  TableColumn,
} from '../../../shared/generic-table/generic-table.component';
import { Filter, SortEvent } from '../../../shared/PaginationRequest/PaginationRequest';
import { RequestHelperService } from '../../../shared/requestHelper/request-helper.service.ts';

@Component({
  selector: 'app-product-component',
  standalone: true,
  imports: [CommonModule, FormsModule, GenericTableComponent],
  templateUrl: './product-component.html',
  styleUrl: './product-component.scss',
})
export class ProductComponent implements OnInit, AfterViewInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  @ViewChild('nameInput') private readonly nameInput?: ElementRef<HTMLInputElement>;
  @ViewChild(GenericTableComponent) private readonly table?: GenericTableComponent<ProductRow>;
  private tableSyncHandle?: number;
  private tableQueryDebounceHandle?: number;
  private lastTableState?: {
    sortBy?: string;
    sortDir?: 'asc' | 'desc';
    page: number;
    pageSize: number;
    query: string;
  };
  private cache: ProductRow[] = [];
  private cacheKey = '';
  private ignoreNextTablePageReset = false;

  loading = true;
  saving = false;
  deletingId?: string;
  error?: string;
  toast?: { type: 'success' | 'error'; message: string };

  products: ProductRow[] = [];
  total = 0;
  page = 1;
  pageSize = 10;
  sort?: SortEvent;
  filter: Filter = {};

  columns: Array<TableColumn<ProductRow>> = [
    {
      id: 'name',
      header: 'Name',
      value: (r) => r.name,
      sortable: true,
      searchable: true,
      width: '34%',
    },
    {
      id: 'color',
      header: 'Color',
      value: (r) => r.color,
      sortable: true,
      searchable: true,
      width: '22%',
    },
    {
      id: 'price',
      header: 'Price',
      value: (r) => r.priceLabel,
      sortable: true,
      searchable: true,
      align: 'right',
      width: '18%',
    },
    {
      id: 'actions',
      header: 'Actions',
      type: 'actions',
      align: 'right',
      width: '26%',
      actions: [
        { id: 'edit', label: 'Edit', variant: 'primary' },
        { id: 'delete', label: 'Delete', variant: 'danger' },
      ],
    },
  ];

  editOpen = false;
  editRow?: ProductRow;
  form: ProductForm = { name: '', color: '', price: null };
  formErrors: Partial<Record<keyof ProductForm, string>> = {};

  constructor(
    private readonly api: Client,
    private readonly requestHelper: RequestHelperService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  ngAfterViewInit(): void {
    const read = () => {
      const t = this.table;
      if (!t) return;

      const next = {
        sortBy: t.sortBy,
        sortDir: t.sortDir,
        page: t.page,
        pageSize: t.pageSize,
        query: t.query ?? '',
      };

      if (!this.lastTableState) {
        this.lastTableState = next;
        return;
      }

      const prev = this.lastTableState;
      this.lastTableState = next;

      if (next.pageSize !== prev.pageSize) {
        this.onPageSizeChange(next.pageSize);
        t.page = 1;
        return;
      }

      if (next.page !== prev.page) {
        if (this.ignoreNextTablePageReset && next.page === 1 && prev.page !== 1) {
          this.ignoreNextTablePageReset = false;
          return;
        }
        this.onPageChange(next.page);
        return;
      }

      if (next.sortBy !== prev.sortBy || next.sortDir !== prev.sortDir) {
        if (t.sortBy) {
          const sort: SortEvent = { sortBy: t.sortBy, sortDirection: t.sortDir };
          t.sortBy = undefined;
          t.sortDir = 'asc';

          this.onSortChange(sort);
        }
        return;
      }

      if (next.query !== prev.query) {
        if (this.tableQueryDebounceHandle) window.clearTimeout(this.tableQueryDebounceHandle);
        this.tableQueryDebounceHandle = window.setTimeout(() => {
          const search = String(next.query ?? '').trim();
          console.log('Products.Search Value', search);
          const filter = this.requestHelper.buildFilterObject({ search });
          console.log('Products.Search Filter', filter);
          this.ignoreNextTablePageReset = true;
          t.page = 1;
          this.onFilterChange(filter);
        }, 250);
      }
    };

    this.tableSyncHandle = window.setInterval(read, 150);
  }

  ngOnDestroy(): void {
    if (this.tableSyncHandle) window.clearInterval(this.tableSyncHandle);
    if (this.tableQueryDebounceHandle) window.clearTimeout(this.tableQueryDebounceHandle);
    this.destroy$.next();
    this.destroy$.complete();
  }

  onTableAction(e: { actionId: string; row: ProductRow }) {
    if (!e?.row?.raw) return;
    if (e.actionId === 'edit') this.openEdit(e.row);
    if (e.actionId === 'delete') this.deleteRow(e.row);
  }

  private deleteRow(row: ProductRow) {
    if (this.deletingId) return;
    const id = row.key;
    const ok = window.confirm('Delete this product?');
    if (!ok) return;

    this.deletingId = id;
    this.closeToast();
    this.api
      .productsDELETE(Number(id))
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.deletingId = undefined;
        }),
      )
      .subscribe({
        next: () => {
          this.toast = { type: 'success', message: 'Product deleted successfully' };
          this.cache = this.cache.filter((p) => p.key !== id);
          this.products = [...this.cache];
          this.total = Math.max(0, this.total - 1);
          this.loadProducts();
        },
        error: (err: any) => {
          console.error('Products.Delete Error', err);
          this.toast = { type: 'error', message: this.errorMessage(err) };
        },
      });
  }

  refresh() {
    this.loadProducts();
  }

  onSortChange(sort?: SortEvent) {
    this.sort = sort;
    this.page = 1;
    this.loadProducts();
  }

  onPageChange(page: number) {
    this.page = page;
    this.ignoreNextTablePageReset = true;
    this.loadProducts();
  }

  onPageSizeChange(pageSize: number) {
    this.pageSize = pageSize;
    this.page = 1;
    this.loadProducts();
  }

  onFilterChange(filter: Filter) {
    this.filter = filter;
    this.page = 1;
    this.loadProducts();
  }

  closeToast() {
    this.toast = undefined;
  }

  openEdit(row: ProductRow) {
    this.closeToast();
    this.formErrors = {};
    this.editRow = row;
    this.form = { name: row.name, color: row.color, price: row.price };
    this.editOpen = true;
    setTimeout(() => this.nameInput?.nativeElement?.focus(), 0);
  }

  openCreate() {
    this.closeToast();
    this.formErrors = {};
    this.editRow = undefined;
    this.form = { name: '', color: '', price: null };
    this.editOpen = true;
    setTimeout(() => this.nameInput?.nativeElement?.focus(), 0);
  }

  closeEdit() {
    if (this.saving) return;
    this.editOpen = false;
    this.editRow = undefined;
  }

  saveEdit() {
    this.closeToast();
    this.formErrors = this.validate(this.form);
    if (Object.keys(this.formErrors).length > 0) return;

    this.saving = true;

    const payload = {
      name: this.form.name.trim(),
      color: this.form.color.trim(),
      price: Number(this.form.price),
    };

    if (!this.editRow) {
      const cmd = new CreateProductCommand(payload);
      this.api
        .create(cmd)
        .pipe(
          takeUntil(this.destroy$),
          finalize(() => {
            this.saving = false;
          }),
        )
        .subscribe({
          next: (res: any) => {
            console.log('Products.Create Response', { success: true });
            this.toast = { type: 'success', message: 'Product created' };
            this.editOpen = false;
            this.loadProducts();
          },
          error: (err: any) => {
            console.error('Products.Create Error', err);
            this.toast = { type: 'error', message: this.errorMessage(err) };
          },
          complete: () => {
            console.log('Products.Create Complete');
          },
        });
      return;
    }

    const id = Number(this.editRow.key);

    const cmd = new UpdateProductCommand(payload);
    console.log('Products.Update Request', { id, ...payload });
    this.api
      .productsPUT(Number(id), cmd)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.saving = false;
        }),
      )
      .subscribe({
        next: (res: unknown) => {
          console.log('Products.Update Response', res);
          this.toast = { type: 'success', message: 'Product updated' };
          this.editOpen = false;
          this.editRow = undefined;
          this.loadProducts();
        },
        error: (err: unknown) => {
          console.error('Products.Update Error', err);
          this.toast = { type: 'error', message: this.errorMessage(err) };
        },
        complete: () => {
          console.log('Products.Update Complete');
        },
      });
  }

  private loadProducts() {
    this.loading = true;
    this.error = undefined;
    const request = this.requestHelper.buildPaginationRequest(
      this.sort,
      this.filter,
      this.page,
      this.pageSize,
    );
    const body = new GetProductsQuery();
    body.page = request.page;
    body.pageSize = request.pageSize;
    body.sort = 'name';
    body.ascending = true;
    console.log(body);
    body.filter = request.filter as any;
    const requestedPage = this.page;
    this.api
      .getAll(body)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.loading = false;
          console.log('Products.GetAll Complete');
        }),
      )
      .subscribe({
        next: (res: ProductDtoListGridResultApiResponse) => {
          console.log('Products.GetAll Response', res);
          if (!res?.success) {
            const msg = String(res?.errors?.[0] ?? res?.message ?? 'Couldn’t load products').trim();
            this.error = msg;
            this.products = [];
            this.total = 0;
            return;
          }

          const raw = res?.data?.data ?? [];
          const rows = this.normalizeRows(raw);
          const total = Number(res?.data?.total ?? 0) || 0;
          const page = requestedPage;

          const key = JSON.stringify({
            pageSize: this.pageSize,
            sort: request.sort ?? '',
            asc: request.ascending ?? false,
            filter: request.filter ?? {},
          });

          if (key !== this.cacheKey || this.cache.length !== total) {
            this.cacheKey = key;
            this.cache = Array.from({ length: total }, (_, i) => this.emptyRow(i));
          }

          const start = Math.max(0, (page - 1) * this.pageSize);
          const mapped = rows.map(this.mapRow);
          for (let i = 0; i < mapped.length; i++) {
            const idx = start + i;
            if (idx >= 0 && idx < this.cache.length) this.cache[idx] = mapped[i];
          }

          this.products = [...this.cache];
          this.total = total;
          console.log('Products.GetAll Page After Response', this.page);
          console.log('Response Total Count:', this.total);
          this.cdr.markForCheck();
        },
        error: (err: unknown) => {
          console.error('Products.GetAll Error', err);
          this.error = this.errorMessage(err);
          this.products = [];
          this.total = 0;
        },
        complete: () => {
          console.log('Products.GetAll Subscriber Complete');
        },
      });
  }

  private emptyRow(i: number): ProductRow {
    return { key: `__empty_${i}`, name: '', color: '', price: 0, priceLabel: '', raw: null };
  }

  private validate(v: ProductForm): Partial<Record<keyof ProductForm, string>> {
    const errors: Partial<Record<keyof ProductForm, string>> = {};
    const name = v.name.trim();
    const color = v.color.trim();
    const price = Number(v.price);

    if (!name) errors.name = 'Required';
    if (!color) errors.color = 'Required';
    if (!Number.isFinite(price) || price <= 0) errors.price = 'Price must be greater than 0';
    return errors;
  }

  private normalizeRows(raw: ProductDto[][] | ProductDto[]): ProductDto[] {
    if (!Array.isArray(raw)) return [];
    const first = (raw as any)[0];
    if (Array.isArray(first)) return (raw as any).flat().filter((x: any) => x);
    return raw as ProductDto[];
  }

  private mapRow = (x: ProductDto, i: number): ProductRow => {
    const name = String(x?.name ?? '').trim();
    const color = String(x?.color ?? '').trim();
    const rawPrice = x?.price ?? null;
    const price = Number(rawPrice);
    const safePrice = Number.isFinite(price) ? price : 0;

    return {
      key: String(x?.id ?? `${i}`),
      name: name || '—',
      color: color || '—',
      price: safePrice,
      priceLabel: this.formatMoney(safePrice),
      raw: x,
    };
  };

  private formatMoney(v: number): string {
    try {
      return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(v);
    } catch {
      return `$${v.toFixed(2)}`;
    }
  }

  private errorMessage(err: unknown): string {
    const anyErr: any = err as any;
    if (typeof anyErr?.message === 'string' && anyErr.message.trim()) return anyErr.message;
    if (typeof anyErr?.error === 'string' && anyErr.error.trim()) return anyErr.error;
    if (typeof anyErr?.error?.message === 'string' && anyErr.error.message.trim())
      return anyErr.error.message;
    return 'Something went wrong';
  }

  private baseUrl(): string {
    const apiAny = this.api as any;
    const v = apiAny?.baseUrl;
    if (typeof v === 'string') return v.replace(/\/$/, '');
    return '';
  }
}

type ProductRow = {
  key: string;
  name: string;
  color: string;
  price: number;
  priceLabel: string;
  raw: unknown;
};

type ProductForm = {
  name: string;
  color: string;
  price: number | null;
};
