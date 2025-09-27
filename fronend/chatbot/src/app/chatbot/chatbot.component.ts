
import { Component, OnInit, ViewChild, ElementRef, AfterViewChecked, Output, EventEmitter } from '@angular/core';
import { ChatbotService } from '../services/chatbot.service';
import { Subscription, timer } from 'rxjs';

interface ChatMessage {
  sender: 'user' | 'bot';
  text: string;
  tooltip?: string;
  timestamp: Date;
  isTyping?: boolean; // add this
}

@Component({
  selector: 'chatbot',
  templateUrl: './chatbot.component.html',
  styleUrls: ['./chatbot.component.scss']
})
export class ChatbotComponent implements OnInit, AfterViewChecked {
  @ViewChild('chatBody') chatBody!: ElementRef;
  @Output() close = new EventEmitter<void>();

  messages: ChatMessage[] = [];
  userInput: string = '';
  loading: boolean = false;
  suggestions: string[] = [];

  showFeedbackForm = false;
  feedback: any = {
    rating: null,
    workedWell: {},
    details: ''
  };
  
  ratingOptions = ['Excellent', 'Good', 'Average', 'Poor'];
  workedWellOptions = ['Clarity', 'Language used', 'Accuracy', 'Relevance', 'Response speed'];


  
  private idleTimer!: any;
  private readonly idleTime = 50000; // 1 min in milliseconds

  // dark mode toggle 
  isDarkMode = false;
toggleDarkMode() {
  this.isDarkMode = !this.isDarkMode;
}

copyMessage(text: string) {
  navigator.clipboard.writeText(text).then(() => {
    alert('Copied to clipboard!');
  });
}

react(msg: any, reaction: string) {
  msg.reaction = reaction; // you can also push multiple reactions in an array
}

@ViewChild('chatContainer') chatContainer!: ElementRef<HTMLDivElement>;
  @ViewChild('dragHandle') dragHandle!: ElementRef<HTMLDivElement>;

  isDragging = false;
  offsetX = 0;
  offsetY = 0;

  isBotTyping = false;

  constructor(private chatbotService: ChatbotService) {}

   // ✅ Add this property for tooltip
  showTooltip: string = '';
  maxLength = 100;
  ngOnInit(): void {}

  ngAfterViewInit(){
     const container = this.chatContainer.nativeElement;
    const header = this.dragHandle.nativeElement;

    header.addEventListener('mousedown', (e: MouseEvent) => {
      this.isDragging = true;
      this.offsetX = e.clientX - container.getBoundingClientRect().left;
      this.offsetY = e.clientY - container.getBoundingClientRect().top;
      document.addEventListener('mousemove', this.onMouseMove);
      document.addEventListener('mouseup', this.onMouseUp);
    });
  }
 onMouseMove = (e: MouseEvent) => {
    if (!this.isDragging) return;
    const container = this.chatContainer.nativeElement;
    let left = e.clientX - this.offsetX;
    let top = e.clientY - this.offsetY;

    // Keep within viewport
    const maxLeft = window.innerWidth - container.offsetWidth;
    const maxTop = window.innerHeight - container.offsetHeight;
    left = Math.max(0, Math.min(left, maxLeft));
    top = Math.max(0, Math.min(top, maxTop));

    container.style.left = left + 'px';
    container.style.top = top + 'px';
    container.style.bottom = 'auto';
    container.style.right = 'auto';
    container.style.position = 'fixed';
  }

  onMouseUp = () => {
    this.isDragging = false;
    document.removeEventListener('mousemove', this.onMouseMove);
    document.removeEventListener('mouseup', this.onMouseUp);
  }
  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  scrollToBottom() {
    if (this.chatBody) {
      this.chatBody.nativeElement.scrollTop = this.chatBody.nativeElement.scrollHeight;
    }
  }

  onInputChange() {
     this.resetIdleTimer();
  if (this.userInput.length > 1) {
    this.chatbotService.getSuggestions(this.userInput).subscribe({
      next: res => this.suggestions = res,
      error: () => this.suggestions = []
    });
  } else {
    this.suggestions = [];
  }
}



  selectSuggestion(s: string) {
    this.userInput = s;
    this.suggestions = [];
    this.sendMessage();
  }

sendMessage() {
  if (!this.userInput.trim()) return;

  // 1️⃣ Add user message
  this.messages.push({
    sender: 'user',
    text: this.userInput,
    timestamp: new Date()
  });
  this.resetIdleTimer();

  const question = this.userInput;
  this.userInput = '';
  this.suggestions = [];

  // 2️⃣ Show bot typing bubble
  const typingBubble: ChatMessage = {
    sender: 'bot',
    text: '',
    timestamp: new Date(),
    isTyping: true
  };
  this.messages.push(typingBubble);

  // 3️⃣ Call chatbot service
  this.chatbotService.askQuestion(question).subscribe({
    next: (response: any) => {
      // Remove typing bubble
      const index = this.messages.indexOf(typingBubble);
      if (index !== -1) this.messages.splice(index, 1);

      // Add actual bot answer
      this.messages.push({
        sender: 'bot',
        text: response.Answer,
        tooltip: response.MatchedFAQ,
        timestamp: new Date()
      });

      this.loading = false;
      this.scrollToBottom();
    },
    error: () => {
      const index = this.messages.indexOf(typingBubble);
      if (index !== -1) this.messages.splice(index, 1);

      this.messages.push({
        sender: 'bot',
        text: '⚠ Error fetching answer. Try again later.',
        timestamp: new Date()
      });

      this.loading = false;
      this.scrollToBottom();
    }
  });

  this.scrollToBottom();
}


// Call this on mouse enter
  setTooltip(msg: ChatMessage) {
    if (msg.sender === 'bot' && msg.tooltip) {
      this.showTooltip = 'Matched FAQ: ' + 
        (msg.tooltip.length > this.maxLength
          ? msg.tooltip.substring(0, this.maxLength) + '…'
          : msg.tooltip);
    } else {
      this.showTooltip = '';
    }
  }

  // Call this on mouse leave
  clearTooltip() {
    this.showTooltip = '';
  }

   // --- Idle timer logic ---
  private startIdleTimer() {
    this.clearIdleTimer();
    this.idleTimer = timer(this.idleTime).subscribe(() => this.onUserIdle());
  }

 resetIdleTimer() {
    clearTimeout(this.idleTimer);
    this.idleTimer = setTimeout(() => {
      this.showFeedbackForm = true;
      this.messages = []; // clear chat after idle
    }, 60000); // 1 minute
  }

  private clearIdleTimer() {
    if (this.idleTimer) this.idleTimer.unsubscribe();
  }

  private onUserIdle() {
    // Show feedback form
    this.showFeedbackForm = true;

    // Clear chat
    this.messages = [];
  }

   submitFeedback() {
    console.log('Feedback submitted:', this.feedback);
    this.showFeedbackForm = false;
    this.feedback = { rating: null, workedWell: {}, details: '' };
  }

  cancelFeedback() {
    this.showFeedbackForm = false;
  }


   ngOnDestroy() {
    this.clearIdleTimer();
  }

}
