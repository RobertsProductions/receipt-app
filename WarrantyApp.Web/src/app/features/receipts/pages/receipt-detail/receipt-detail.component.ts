import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { CardComponent } from '../../../../shared/components/card/card.component';
import { InputComponent } from '../../../../shared/components/input/input.component';
import { BadgeComponent } from '../../../../shared/components/badge/badge.component';
import { ModalComponent } from '../../../../shared/components/modal/modal.component';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';
import { ReceiptService } from '../../../../services/receipt.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { Receipt } from '../../../../models';

@Component({
  selector: 'app-receipt-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    ButtonComponent,
    CardComponent,
    InputComponent,
    BadgeComponent,
    ModalComponent,
    SpinnerComponent
  ],
  templateUrl: './receipt-detail.component.html',
  styleUrl: './receipt-detail.component.scss'
})
export class ReceiptDetailComponent implements OnInit {
  receipt: Receipt | null = null;
  loading: boolean = true;
  showEditModal: boolean = false;
  editForm: FormGroup;
  saving: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private receiptService: ReceiptService,
    private toast: ToastService,
    private fb: FormBuilder
  ) {
    this.editForm = this.fb.group({
      merchantName: ['', Validators.required],
      totalAmount: ['', [Validators.required, Validators.min(0)]],
      purchaseDate: ['', Validators.required],
      productName: [''],
      warrantyMonths: ['', Validators.min(0)],
      notes: ['']
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.loadReceipt(params['id']);
    });
  }

  loadReceipt(id: string): void {
    this.loading = true;
    this.receiptService.getReceipt(id).subscribe({
      next: (receipt) => {
        this.receipt = receipt;
        this.loading = false;
      },
      error: () => {
        this.toast.error('Receipt not found');
        this.router.navigate(['/receipts']);
      }
    });
  }

  openEditModal(): void {
    if (!this.receipt) return;
    
    this.editForm.patchValue({
      merchantName: this.receipt.merchant,
      totalAmount: this.receipt.amount,
      purchaseDate: this.receipt.purchaseDate?.split('T')[0],
      productName: this.receipt.productName,
      warrantyMonths: this.receipt.warrantyMonths,
      notes: this.receipt.notes
    });
    
    this.showEditModal = true;
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.editForm.reset();
  }

  saveReceipt(): void {
    if (this.editForm.invalid || !this.receipt) {
      this.editForm.markAllAsTouched();
      return;
    }

    this.saving = true;
    const formData = this.editForm.value;
    const updateData = {
      merchant: formData.merchantName,
      amount: formData.totalAmount,
      purchaseDate: formData.purchaseDate,
      productName: formData.productName,
      warrantyMonths: formData.warrantyMonths,
      notes: formData.notes
    };

    this.receiptService.updateReceipt(this.receipt.id, updateData).subscribe({
      next: (updated) => {
        this.receipt = updated;
        this.toast.success('Receipt updated successfully');
        this.closeEditModal();
        this.saving = false;
      },
      error: () => {
        this.toast.error('Failed to update receipt');
        this.saving = false;
      }
    });
  }

  deleteReceipt(): void {
    if (!this.receipt) return;
    
    if (!confirm('Are you sure you want to delete this receipt? This cannot be undone.')) {
      return;
    }

    this.receiptService.deleteReceipt(this.receipt.id).subscribe({
      next: () => {
        this.toast.success('Receipt deleted');
        this.router.navigate(['/receipts']);
      },
      error: () => {
        this.toast.error('Failed to delete receipt');
      }
    });
  }

  runOCR(): void {
    if (!this.receipt) return;

    this.receiptService.processOcr(this.receipt.id).subscribe({
      next: () => {
        this.toast.success('OCR processing complete');
        this.loadReceipt(this.receipt!.id);
      },
      error: () => {
        this.toast.error('OCR processing failed');
      }
    });
  }

  getWarrantyBadge(): { variant: 'success' | 'warning' | 'error' | 'neutral', text: string } | null {
    if (!this.receipt?.warrantyExpirationDate) return null;

    const expiryDate = new Date(this.receipt.warrantyExpirationDate);
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

  get formErrors() {
    return {
      merchantName: this.getFieldError('merchantName'),
      totalAmount: this.getFieldError('totalAmount'),
      purchaseDate: this.getFieldError('purchaseDate')
    };
  }

  private getFieldError(fieldName: string): string | undefined {
    const control = this.editForm.get(fieldName);
    if (control?.touched && control?.invalid) {
      if (control.errors?.['required']) return `${fieldName} is required`;
      if (control.errors?.['min']) return `${fieldName} must be positive`;
    }
    return undefined;
  }
}
