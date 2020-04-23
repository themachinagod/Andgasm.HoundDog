// #region Imports
import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { UserAvatarService } from '../../shared/services/user.avatar.service';
import { NgbModal, NgbActiveModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { ImageCroppedEvent, ImageCropperComponent } from 'ngx-image-cropper';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';
// #endregion

@Component({
  selector: 'avatar-upload',
  templateUrl: './avatar-upload.component.html',
  styleUrls: ['./avatar-upload.component.css']
})
export class AvatarUploadComponent implements OnInit {

  // #region Form Elements
  @ViewChild(ImageCropperComponent, { static: false }) imageCropper: ImageCropperComponent;
  // #endregion

  // #region Fields
  errors: [];
  isRequesting: boolean = false;
  hasSelectedImage: boolean = false;
  croppedImage: any = '';
  imageChangedEvent: Event;
  previewModal: NgbModalRef;
  // #endregion

  // #region Output Events
  @Output() modalEventEmitter: EventEmitter<any> = new EventEmitter();
  // #endregion

  // #region Constructor
  constructor(
    private _avatarService: UserAvatarService,
    private _toastrService: ToastrService,
    private _modalService: NgbModal,
    private _activeModal: NgbActiveModal) {

    this.errors = [];
  }
  //#endregion

  // #region Init/Destroy
  ngOnInit() {
  }
  // #endregion

  // #region Cropping Events
  fileChangeEvent(event: Event): void {

    this.imageChangedEvent = event;
  }

  imageCropped(event: ImageCroppedEvent) {
  
    this.croppedImage = event.base64;
  }

  cropperReady() {

    this.imageCropper.cropper = { x1: 0, y1: 0, x2: 360, y2: 360 };
  }

  imageLoaded() {

    this.hasSelectedImage = true;
  }
  // #endregion

  // #region Preview
  openPreviewModal(content: any) {

    this.previewModal = this._modalService.open(content, { windowClass: 'modal-400' });
  }
  // #endregion

  // #region Upload
  uploadAvatar() {

    const imageBlob = this.dataURItoBlob(this.croppedImage);
    this._avatarService.uploadAvatar(imageBlob)
      .pipe(finalize(() => this.isRequesting = false))
      .subscribe(
        result => {
          if (result) {
            this._toastrService.success('You have successfully uploaded your new avatar and associated to your account! Now everyone can see how good you look.', 'Avatar Uploaded!');
            this.modalEventEmitter.emit(this.croppedImage);
            this._activeModal.close();
          }
        },
        errors => {
          this._toastrService.error('Unfortunatley your avatar could not be uploaded, please try again! If this problem continues please contact a member of our support team.', 'Avatar Upload Failed!');
          this.errors = errors;
        });
  }

  dataURItoBlob(dataURI) {

    var santiseddatauri = dataURI.replace('data:image/png;base64,', '')
    const byteString = window.atob(santiseddatauri);
    const arrayBuffer = new ArrayBuffer(byteString.length);
    const int8Array = new Uint8Array(arrayBuffer);
    for (let i = 0; i < byteString.length; i++) {
      int8Array[i] = byteString.charCodeAt(i);
    }
    const blob = new Blob([int8Array], { type: 'image/png' });
    return blob;
  }
  // #endregion
}
