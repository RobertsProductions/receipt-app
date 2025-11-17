import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { CardComponent } from '../../../../shared/components/card/card.component';
import { BadgeComponent } from '../../../../shared/components/badge/badge.component';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { ReceiptService } from '../../../../services/receipt.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { Receipt } from '../../../../models';

interface WarrantySummary {
  total: number;
  expiringSoon: number;
  valid: number;
  expired: number;
}

interface WarrantyItem extends Receipt {
  daysUntilExpiry: number;
  urgency: 'critical' | 'warning' | 'normal';
}

@Component({
  selector: 'app-warranty-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ButtonComponent,
    CardComponent,
    BadgeComponent,
    SpinnerComponent,
    EmptyStateComponent
  ],
  templateUrl: './warranty-dashboard.component.html',
  styleUrl: './warranty-dashboard.component.scss'
})
export class WarrantyDashboardComponent implements OnInit {
  loading: boolean = false;
  warranties: WarrantyItem[] = [];
  filteredWarranties: WarrantyItem[] = [];
  summary: WarrantySummary = { total: 0, expiringSoon: 0, valid: 0, expired: 0 };
  selectedFilter: number = 30; // days

  constructor(
    private receiptService: ReceiptService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.loadWarranties();
  }

  loadWarranties(): void {
    this.loading = true;
    this.receiptService.getReceipts(1, 100).subscribe({
      next: (response) => {
        const today = new Date();
        this.warranties = response.receipts
          .filter(r => r.warrantyExpirationDate)
          .map(r => {
            const expiryDate = new Date(r.warrantyExpirationDate!);
            const daysUntilExpiry = Math.ceil((expiryDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
            let urgency: 'critical' | 'warning' | 'normal' = 'normal';
            if (daysUntilExpiry < 0) urgency = 'critical';
            else if (daysUntilExpiry <= 7) urgency = 'critical';
            else if (daysUntilExpiry <= 30) urgency = 'warning';
            
            return { ...r, daysUntilExpiry, urgency } as WarrantyItem;
          })
          .sort((a, b) => a.daysUntilExpiry - b.daysUntilExpiry);

        this.calculateSummary();
        this.applyFilter(this.selectedFilter);
        this.loading = false;
      },
      error: () => {
        this.toast.error('Failed to load warranties');
        this.loading = false;
      }
    });
  }

  calculateSummary(): void {
    this.summary.total = this.warranties.length;
    this.summary.expired = this.warranties.filter(w => w.daysUntilExpiry < 0).length;
    this.summary.expiringSoon = this.warranties.filter(w => w.daysUntilExpiry >= 0 && w.daysUntilExpiry <= 30).length;
    this.summary.valid = this.warranties.filter(w => w.daysUntilExpiry > 30).length;
  }

  applyFilter(days: number): void {
    this.selectedFilter = days;
    if (days === -1) {
      this.filteredWarranties = this.warranties;
    } else {
      this.filteredWarranties = this.warranties.filter(w => 
        w.daysUntilExpiry >= 0 && w.daysUntilExpiry <= days
      );
    }
  }

  getUrgencyBadge(warranty: WarrantyItem): { variant: 'success' | 'warning' | 'error', text: string } {
    if (warranty.daysUntilExpiry < 0) {
      return { variant: 'error', text: 'Expired' };
    } else if (warranty.daysUntilExpiry === 0) {
      return { variant: 'error', text: 'Expires Today!' };
    } else if (warranty.daysUntilExpiry <= 7) {
      return { variant: 'error', text: `${warranty.daysUntilExpiry} days left` };
    } else if (warranty.daysUntilExpiry <= 30) {
      return { variant: 'warning', text: `${warranty.daysUntilExpiry} days left` };
    } else {
      return { variant: 'success', text: `${warranty.daysUntilExpiry} days left` };
    }
  }

  getUrgencyIcon(warranty: WarrantyItem): string {
    return warranty.daysUntilExpiry <= 7 ? '🚨' : warranty.daysUntilExpiry <= 30 ? '⚠️' : '✓';
  }
}
