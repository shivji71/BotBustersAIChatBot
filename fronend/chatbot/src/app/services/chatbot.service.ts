import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '.././../environments/environment'; // <-- use Angular environment

interface FAQ {
  Question: string;
}

interface FAQData {
  FAQs: FAQ[];
}

@Injectable({
  providedIn: 'root'
})
export class ChatbotService {
  private apiUrl = environment.apiUrl;       // use environment
  private suggestUrl = `${environment.apiUrl}/ChatGPTBotController/Suggest`;
  private faqUrl = environment.faqUrl;       // use environment

  constructor(private http: HttpClient) {}

  askQuestion(question: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/ChatGPTBotController/Ask`, { question });
  }

  getSuggestions(query: string): Observable<string[]> {
    return this.http.get<FAQData>(this.faqUrl).pipe(
      map(data =>
        data.FAQs
          .map(f => f.Question)
          .filter(q => q.toLowerCase().includes(query.toLowerCase()))
      )
    );
  }

  uploadDocument(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post(`${this.apiUrl}/ChatGPTBotController/uploadDocument`, formData);
  }
}
