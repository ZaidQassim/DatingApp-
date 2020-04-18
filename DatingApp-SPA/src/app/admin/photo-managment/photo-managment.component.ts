import { Component, OnInit } from '@angular/core';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-photo-managment',
  templateUrl: './photo-managment.component.html',
  styleUrls: ['./photo-managment.component.css']
})
export class PhotoManagmentComponent implements OnInit {

  photos: any;

  constructor(private adminService: AdminService) { }

  ngOnInit() {
    this.getPhotosForApproval();
  }

  getPhotosForApproval() {
    this.adminService.getPhotosForApproval().subscribe((photos) => {
      this.photos = photos;   
    },  
    error =>{
      console.log(error);
    });
  }


  approvalPhoto(photoId){
    this.adminService.approvePhoto(photoId)
    .subscribe(() => {
      this.photos.splice(this.photos.findIndex(p => p.id === photoId), 1);
    },  
    error => {
      console.log(error);
    });
  }



 

  recjectPhoto(photoId){
    this.adminService.rejectPhoto(photoId)
    .subscribe(() => {
      this.photos.splice(this.photos.findIndex(p => p.id === photoId), 1);
    },
     error =>{
      console.log(error);
    });
  }









}
