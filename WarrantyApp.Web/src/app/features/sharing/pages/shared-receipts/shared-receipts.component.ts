import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CardComponent } from '../../../../shared/components/card/card.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { BadgeComponent } from '../../../../shared/components/badge/badge.component';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';
import { ReceiptService } from '../../../../services/receipt.service';
import { ToastService } from '../../../../shared/services/toast.service';

interface SharedReceipt {
  id: number;
  merchantName: string;
  purchaseDate: string;
  totalAmount: number;
  productName?: string;
  warrantyEndDate?: string;
  ownerEmail: string;
  sharedBy: string;
  sharedAt: string;
  accessLevel: 'view';
}

@Component({
  selector: 'app-shared-receipts',
  standalone: true,
  imports: [CommonModule, CardComponent, EmptyStateComponent, BadgeComponent, SpinnerComponent],
  templateUrl: './shared-receipts.component.html',
  styleUrl: './shared-receipts.component.scss'
})
export class SharedReceiptsComponent implements OnInit {
  receipts: SharedReceipt[] = [];
  loading = true;

  constructor(
    private receiptService: ReceiptService,
    private toastService: ToastService,
    private router: Router
  ) {}

  async ngOnInit() {
    await this.loadSharedReceipts();
  }

  async loadSharedReceipts() {
    this.loading = true;

    try {
      const response = await this.receiptService.getSharedWithMe().toPromise();
      this.receipts = response || [];
    } catch (error: any) {
      this.toastService.error(error.error?.message || 'Failed to load shared receipts');
      this.receipts = [];
    } finally {
      this.loading = false;
    }
  }

  viewReceipt(receiptId: number) {
    this.router.navigate(['/receipts', receiptId]);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  getWarrantyStatus(warrantyEndDate?: string): 'active' | 'expiring' | 'expired' | null {
    if (!warrantyEndDate) return null;

    const today = new Date();
    const endDate = new Date(warrantyEndDate);
    const daysUntilExpiry = Math.floor((endDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));

    if (daysUntilExpiry < 0) return 'expired';
    if (daysUntilExpiry <= 30) return 'expiring';
    return 'active';
  }

  getWarrantyBadgeVariant(status: string | null): 'success' | 'warning' | 'error' | 'default' {
    switch (status) {
      case 'active': return 'success';
      case 'expiring': return 'warning';
      case 'expired': return 'error';
      default: return 'default';
    }
  }
}
