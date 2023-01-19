import { Component } from '@angular/core';
import { Photo } from 'src/app/models/photo';
import { AdminService } from 'src/app/services/admin.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent {
  photos: Photo[];

  constructor(private adminService: AdminService) { }

  ngOnInit(): void {
    this.getPhotoForApproval();
  }

  getPhotoForApproval() {
    this.adminService.getPhotosForApproval().subscribe(photos => {
      this.photos = photos;
    })
  }

  approvePhoto(photoId) {
    this.adminService.approvePhoto(photoId).subscribe(() => {
      this.photos.splice(this.photos.findIndex(p => p.id === photoId, 1))
    })
  }

  rejectPhoto(photoId) {
    this.adminService.rejectPhoto(photoId).subscribe(() => {
      this.photos.splice(this.photos.findIndex(p => p.id === photoId), 1)
    })
  }
}
