import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { NotFoundComponent } from './errors/not-found/not-found.component';
import { ServerErrorComponent } from './errors/server-error/server-error.component';
import { TestErrorsComponent } from './errors/test-errors/test-errors.component';
import { HomeComponent } from './home/home.component';
import { ListsComponent } from './lists/lists.component';
import { MemberDetailComponent } from './members/member-detail/member-detail.component';
import { MemberListComponent } from './members/member-list/member-list.component';
import { MessagesComponent } from './messages/messages.component';
import { AuthGuard } from './_guards/auth.guard';

const routes: Routes = [
  {path: '', component: HomeComponent}, //when we browser to https://localhost:4200 - it will see the HomeComponent
  //make an general rule for all these components
  {path: '', runGuardsAndResolvers: 'always', canActivate: [AuthGuard], children: [
    {path: 'members', component: MemberListComponent},
    {path: 'members/:username', component: MemberDetailComponent}, //.../members/(can be 1, 2, or 3...)
    {path: 'lists', component: ListsComponent},
    {path: 'messages', component: MessagesComponent}
  ]},
  {path: 'errors', component: TestErrorsComponent},
  {path: 'not-found', component: NotFoundComponent},
  {path: 'server-error', component: ServerErrorComponent},
  {path: '**', component: NotFoundComponent, pathMatch: 'full'} //when users does not write anything in this array - then redirect to ** HomeComponent
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
