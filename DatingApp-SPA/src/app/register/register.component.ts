import { Component, OnInit, Input, Output, EventEmitter } from "@angular/core";
import { AuthService } from "../_services/auth.service";
import { AlertifyService } from "../_services/alertify.service";
import { FormGroup, FormControl, Validators, FormBuilder } from "@angular/forms";
import { BsDatepickerConfig } from 'ngx-bootstrap';
import { User } from '../_models/user';
import { Router } from '@angular/router';

@Component({
  selector: "app-register",
  templateUrl: "./register.component.html",
  styleUrls: ["./register.component.css"]
})
export class RegisterComponent implements OnInit {
  /* يستقبل بينات من componet اخر   */
  /* @Input() valuesFromHomeComponet :any; */
  @Output() cancelRegister = new EventEmitter();

  user: User ;
  registerForm: FormGroup;

  // to  changes theme style for  Date picker 
  bsConfig: Partial<BsDatepickerConfig>;

  constructor(
    private authService: AuthService,
    private alertify: AlertifyService,
    private router: Router,
    private fb: FormBuilder
  ) {}

  ngOnInit() {
    //  toc change the propirtes od data picker 
    this.bsConfig ={
      containerClass :"theme-red"
    },
    this.createRegisterForm();
  }

  // to create the model register usres
  createRegisterForm(){
    this.registerForm = this.fb.group({
      gender: ["male"],
      userName:["", Validators.required],
      knownAs:["Hello", Validators.required],
      dateOfBirth:[null, Validators.required],
      city:["", Validators.required],
      country:["", Validators.required],
      password: ["", [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ["", Validators.required]
    }, 
    { validators: this.passwordMatchValidator });
  }

  // to  valdation of password and confirm password 
  passwordMatchValidator(g: FormGroup){
    return g.get("password").value === g.get("confirmPassword").value ? null : {"mismatch": true}
  }



  register() {
    if(this.registerForm.valid){
      this.user = Object.assign({}, this.registerForm.value);
      this.authService.register(this.user).subscribe(() => {
        this.alertify.success("Registeration successful ");
      }, 
      error => {
        this.alertify.error(error.value);
      },
      () => {
        this.authService.login(this.user).subscribe( () => {
          this.router.navigate(["/members"]);
        });
      });
    }

  }

  // button cansoel 
  cansoel() {
    this.cancelRegister.emit(false);
    console.log("Cancelled");
  }

  
}
