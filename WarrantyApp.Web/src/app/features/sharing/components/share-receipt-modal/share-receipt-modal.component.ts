import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ModalComponent } from '../../../../shared/components/modal/modal.component';
import { InputComponent } from '../../../../shared/components/input/input.component';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../../shared/components/badge/badge.component';
import { ReceiptService } from '../../../../services/receipt.service';
import { ToastService } from '../../../../shared/services/toast.service';

interface SharedUser {
  email: string;
  sharedAt: string;
  accessLevel: 'view';
}

@Component({
  selector: 'app-share-receipt-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, ModalComponent, InputComponent, ButtonComponent, BadgeComponent],
  templateUrl: './share-receipt-modal.component.html',
  styleUrl: './share-receipt-modal.component.scss'
})
export class ShareReceiptModalComponent implements OnInit {
  @Input() isOpen = false;
  @Input() receiptId?: number;
  @Output() closeModal = new EventEmitter<void>();

  emailToShare = '';
  sharedUsers: SharedUser[] = [];
  loading = false;
  shareLoading = false;

  constructor(
    private receiptService: ReceiptService,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    if (this.receiptId) {
      this.loadSharedUsers();
    }
  }

  async loadSharedUsers() {
    if (!this.receiptId) return;

    this.loading = true;

    try {
      const response = await this.receiptService.getReceiptShares(this.receiptId).toPromise();
      this.sharedUsers = response || [];
    } catch (error: any) {
      this.toastService.error(error.error?.message || 'Failed to load shared users');
    } finally {
      this.loading = false;
    }
  }

  async shareReceipt() {
    if (!this.emailToShare || !this.receiptId) {
      this.toastService.error('Please enter an email address');
      return;
    }

    // Basic email validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.emailToShare)) {
      this.toastService.error('Please enter a valid email address');
      return;
    }

    // Check if already shared
    if (this.sharedUsers.some(u => u.email.toLowerCase() === this.emailToShare.toLowerCase())) {
      this.toastService.error('Receipt already shared with this user');
      return;
    }

    this.shareLoading = true;

    try {
      await this.receiptService.shareReceipt(this.receiptId, this.emailToShare).toPromise();
      this.toastService.success(`Receipt shared with ${this.emailToShare}`);
      this.emailToShare = '';
      await this.loadSharedUsers();
    } catch (error: any) {
      this.toastService.error(error.error?.message || 'Failed to share receipt');
    } finally {
      this.shareLoading = false;
    }
  }

  async revokeAccess(email: string) {
    if (!this.receiptId) return;

    const confirmed = confirm(`Remove access for ${email}?`);
    if (!confirmed) return;

    try {
      await this.receiptService.revokeShare(this.receiptId, email).toPromise();
      this.toastService.success('Access revoked');
      await this.loadSharedUsers();
    } catch (error: any) {
      this.toastService.error(error.error?.message || 'Failed to revoke access');
    }
  }

  close() {
    this.closeModal.emit();
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
