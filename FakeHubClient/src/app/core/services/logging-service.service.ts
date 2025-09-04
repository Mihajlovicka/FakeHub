import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Path } from "../constant/path";

@Injectable({ providedIn: 'root' })
export class LoggingService {
  private http: HttpClient = inject(HttpClient);
  
  public logError(message: string, stack?: string) : void {
    this.http.post(Path.Log, { message, stack }).subscribe();
  }
}