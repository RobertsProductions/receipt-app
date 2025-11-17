import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { CardComponent } from '../../../../shared/components/card/card.component';
import { InputComponent } from '../../../../shared/components/input/input.component';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { PaginationComponent } from '../../../../shared/components/pagination/pagination.component';
import { BadgeComponent } from '../../../../shared/components/badge/badge.component';
import { ModalComponent } from '../../../../shared/components/modal/modal.component';
import { FileUploadComponent } from '../../../../shared/components/file-upload/file-upload.component';
import { ReceiptService } from '../../../../services/receipt.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { Receipt } from '../../../../models';

@Component({
  selector: 'app-receipt-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    ButtonComponent,
    CardComponent,
    InputComponent,
    SpinnerComponent,
    EmptyStateComponent,
    PaginationComponent,
    BadgeComponent,
    ModalComponent,
    FileUploadComponent
  ],
  templateUrl: './receipt-list.component.html',
  styleUrl: './receipt-list.component.scss'
})
export class ReceiptListComponent implements OnInit {
  receipts: Receipt[] = [];
  loading: boolean = false;
  currentPage: number = 1;
  pageSize: number = 12;
  totalCount: number = 0;
  searchQuery: string = '';
  showUploadModal: boolean = false;
  selectedFiles: File[] = [];
  uploadingOCR: boolean = false;
  uploading: boolean = false;

  constructor(
    private receiptService: ReceiptService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.loadReceipts();
  }

  loadReceipts(): void {
    this.loading = true;
    this.receiptService.getReceipts(this.currentPage, this.pageSize).subscribe({
      next: (response) => {
        this.receipts = response.receipts;
        this.totalCount = response.totalCount;
        this.loading = false;
      },
      error: (err) => {
        console.error('❌ Failed to load receipts:', err);
        console.error('Status:', err.status);
        console.error('Message:', err.message);
        console.error('Full error:', err);
        this.toast.error(`Failed to load receipts: ${err.status} ${err.statusText || err.message}`);
        this.loading = false;
      }
    });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadReceipts();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  openUploadModal(): void {
    this.showUploadModal = true;
  }

  closeUploadModal(): void {
    this.showUploadModal = false;
    this.selectedFiles = [];
  }

  onFilesSelected(files: File[]): void {
    this.selectedFiles = files;
  }

  uploadReceipt(): void {
    if (this.selectedFiles.length === 0) {
      this.toast.error('Please select a file');
      return;
    }

    this.uploading = true;
    const file = this.selectedFiles[0];

    this.receiptService.uploadReceipt(file).subscribe({
      next: (receipt) => {
        if (this.uploadingOCR) {
          this.toast.info('Receipt uploaded. Processing OCR...');
          this.processOCR(receipt.id);
        } else {
          this.toast.success('Receipt uploaded successfully!');
          this.closeUploadModal();
          this.loadReceipts();
        }
      },
      error: () => {
        this.toast.error('Failed to upload receipt');
        this.uploading = false;
      }
    });
  }

  processOCR(receiptId: string): void {
    this.receiptService.processOcr(receiptId).subscribe({
      next: () => {
        this.toast.success('Receipt uploaded and processed!');
        this.uploading = false;
        this.closeUploadModal();
        this.loadReceipts();
      },
      error: () => {
        this.toast.warning('Receipt uploaded, but OCR processing failed');
        this.uploading = false;
        this.closeUploadModal();
        this.loadReceipts();
      }
    });
  }

  getWarrantyBadge(receipt: Receipt): { variant: 'success' | 'warning' | 'error' | 'neutral', text: string } | null {
    if (!receipt.warrantyExpirationDate) return null;

    const expiryDate = new Date(receipt.warrantyExpirationDate);
    const today = new Date();
    const daysUntilExpiry = Math.ceil((expiryDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));

    if (daysUntilExpiry < 0) {
      return { variant: 'error', text: 'Expired' };
    } else if (daysUntilExpiry <= 30) {
      return { variant: 'warning', text: `Expires in ${daysUntilExpiry} days` };
    } else {
      return { variant: 'success', text: 'Active' };
    }
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  deleteReceipt(id: string): void {
    if (!confirm('Are you sure you want to delete this receipt?')) return;

    this.receiptService.deleteReceipt(id).subscribe({
      next: () => {
        this.toast.success('Receipt deleted');
        this.loadReceipts();
      },
      error: () => {
        this.toast.error('Failed to delete receipt');
      }
    });
  }
}
