import { ErrorHandler, Injectable } from '@angular/core';
import { LoggingService } from '../services/logging-service.service';

@Injectable({ providedIn: 'root' })
export class GlobalErrorHandler implements ErrorHandler {
  constructor(private loggingService: LoggingService) {}

  handleError(error: any): void {
    const message = error.message || error.toString();
    const stack = error.stack || null;
    this.loggingService.logError(message, stack);
    console.error(error);
  }
}
