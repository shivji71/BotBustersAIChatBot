import { Component } from '@angular/core';
import { ChatbotService } from './services/chatbot.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  chatOpen = false;

  constructor(private chatbotService: ChatbotService) {}

  toggleChat() {
    this.chatOpen = !this.chatOpen;
  }


  onFileSelected(event: any) {
  const file: File = event.target.files[0];
  if (file) {
    this.chatbotService.uploadDocument(file).subscribe({
      next: (res) => {
        alert(res.message); // success message
      },
      error: (err) => {
        console.error(err);
        alert("⚠ Failed to upload file.");
      }
    });
  }
}
  
}
