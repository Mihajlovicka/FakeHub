import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class StorageService {
    public getItem(key: string): string | null {
        if (typeof localStorage !== 'undefined') {
            return localStorage.getItem(key);
        }
        return null;
    }

    public setItem(key: string, value: string): void {
        if (typeof localStorage !== 'undefined') {
            localStorage.setItem(key, value);
        }
    }

    public removeItem(key: string): void {
        if (typeof localStorage !== 'undefined') {
            localStorage.removeItem(key);
        }
    }
}
