import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { PhotoEditorComponent } from '../members/photo-editor/photo-editor.component';
import { Photo } from '../models/photo';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getUsersWithRoles(){
    return this.http.get<User[]>(this.baseUrl + 'admin/users-with-roles')
  }

  updateUserRolse(username: string, roles: string) {
    return this.http.post<string[]>(this.baseUrl + 'admin/edit-roles/' + username + '?roles=' + roles, {});
  }

  getPhotosForApproval(){
    return this.http.get<Photo[]>(this.baseUrl+ 'admin/photos-to-moderate');
  }

  approvePhoto(photoId: number){
    return this.http.post(this.baseUrl + 'admin/approve-photo/' + photoId, {});
  }

  rejectPhoto(photoId: number){
    return this.http.post(this.baseUrl + 'admin/reject-photo/' + photoId, {});
  }
}
