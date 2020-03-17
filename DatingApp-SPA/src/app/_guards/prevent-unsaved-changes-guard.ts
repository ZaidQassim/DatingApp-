import { Injectable } from "@angular/core";
import { CanDeactivate } from "@angular/router";
import { MemberEditComponent } from "../members/member-edit/member-edit.component";

@Injectable()

export class PreventUnsavedChanges implements CanDeactivate<MemberEditComponent> {
  canDeactivate(component: MemberEditComponent){
    if (component.editForm.dirty) {
      return confirm("Are you sure want to  continue? Any unsaved changes wil  l be loast");
    }
    return true;
  }
}
