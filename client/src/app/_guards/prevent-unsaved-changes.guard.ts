import { Injectable } from '@angular/core';
import { CanDeactivate } from '@angular/router';
import { Observable } from 'rxjs';
import { MemberEditComponent } from '../members/member-edit/member-edit.component';
import { ConfirmService } from '../_services/confirm.service';

@Injectable({
  providedIn: 'root'
})

//it is intended to save any changes user made in Edit Profile even if they do not press button Save Changes and go to another route
export class PreventUnsavedChangesGuard implements CanDeactivate<unknown> {
  constructor(private confirmService: ConfirmService) {}

  //in guard observables automarically subscribes 
  canDeactivate(component: MemberEditComponent): Observable<boolean> | boolean {
    if (component.editForm.dirty) {
      return this.confirmService.confirm('Confirm unsaved data', 'Are you sure you want to leave?', 'Yes', 'No');
    }
    return true;
  }
  
}
