import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { QuestBoardDto } from './quest-board.models';

@Injectable({
  providedIn: 'root'
})
export class QuestBoardService {
  private readonly API_URL = `${environment.apiUrl}/quests`;

  constructor(private http: HttpClient) {}

  /**
   * Gets all active quests. If user is authenticated, includes progress.
   */
  getQuestBoard(): Observable<QuestBoardDto[]> {
    return this.http.get<QuestBoardDto[]>(this.API_URL).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: any) {
    let errorMessage = 'Unknown error occurred';
    if (error.error && error.error.message) {
      errorMessage = error.error.message;
    } else if (error.message) {
      errorMessage = error.message;
    } else {
      errorMessage = `Server error: ${error.status}`;
    }
    console.error('QuestBoardService error:', errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}
