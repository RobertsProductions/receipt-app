import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-file-upload',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './file-upload.component.html',
  styleUrl: './file-upload.component.scss'
})
export class FileUploadComponent {
  @Input() accept: string = 'image/*,application/pdf';
  @Input() multiple: boolean = false;
  @Input() maxSize: number = 10 * 1024 * 1024; // 10MB
  @Input() maxFiles?: number;
  @Output() filesSelected = new EventEmitter<File[]>();

  isDragging: boolean = false;
  selectedFiles: File[] = [];
  errors: string[] = [];

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;

    const files = Array.from(event.dataTransfer?.files || []);
    this.handleFiles(files);
  }

  onFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files || []);
    this.handleFiles(files);
  }

  handleFiles(files: File[]): void {
    this.errors = [];
    const validFiles: File[] = [];

    for (const file of files) {
      // Check file size
      if (file.size > this.maxSize) {
        this.errors.push(`${file.name} is too large (max ${this.formatBytes(this.maxSize)})`);
        continue;
      }

      // Check file type
      const acceptedTypes = this.accept.split(',').map(t => t.trim());
      const fileType = file.type;
      const fileName = file.name.toLowerCase();
      const fileExtension = fileName.substring(fileName.lastIndexOf('.'));
      
      const isAccepted = acceptedTypes.some(type => {
        if (type.endsWith('/*')) {
          // Handle wildcard types like "image/*"
          return fileType.startsWith(type.replace('/*', ''));
        }
        return fileType === type;
      });

      // Fallback: check by file extension if MIME type check fails
      // This handles cases where browser doesn't set proper MIME type
      const extensionAccepted = !fileType && acceptedTypes.some(type => {
        if (type === 'application/pdf' && fileExtension === '.pdf') return true;
        if (type === 'image/*' && ['.jpg', '.jpeg', '.png', '.gif', '.webp'].includes(fileExtension)) return true;
        return false;
      });

      if (!isAccepted && !extensionAccepted) {
        this.errors.push(`${file.name} is not an accepted file type. Accepted: JPEG, PNG, PDF`);
        continue;
      }

      validFiles.push(file);
    }

    // Check max files
    if (this.maxFiles && validFiles.length > this.maxFiles) {
      this.errors.push(`Maximum ${this.maxFiles} file(s) allowed`);
      return;
    }

    if (validFiles.length > 0) {
      this.selectedFiles = this.multiple ? [...this.selectedFiles, ...validFiles] : validFiles;
      this.filesSelected.emit(this.selectedFiles);
    }
  }

  removeFile(index: number): void {
    this.selectedFiles.splice(index, 1);
    this.filesSelected.emit(this.selectedFiles);
  }

  formatBytes(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  getFileIcon(file: File): string {
    if (file.type.startsWith('image/')) return '🖼️';
    if (file.type === 'application/pdf') return '📄';
    return '📎';
  }
}
