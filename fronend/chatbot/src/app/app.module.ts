import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms'; // 👈 add this
import { HttpClientModule } from '@angular/common/http'; // 👈 needed for API calls
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ChatbotComponent } from './chatbot/chatbot.component';
import { BotbusterComponent } from './botbuster/botbuster.component';

@NgModule({
  declarations: [
    AppComponent,
    ChatbotComponent,
    BotbusterComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    HttpClientModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
