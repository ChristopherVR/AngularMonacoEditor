import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BaseEditorComponent } from './base-editor/base-editor';
import { MonacoEditorComponent } from './monaco-editor/monaco-editor.component';

@NgModule({
  declarations: [AppComponent, BaseEditorComponent, MonacoEditorComponent],
  imports: [BrowserModule, AppRoutingModule, BrowserAnimationsModule],
  providers: [],
  bootstrap: [AppComponent],
})
export class AppModule {}
