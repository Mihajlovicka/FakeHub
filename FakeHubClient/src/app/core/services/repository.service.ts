import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { BehaviorSubject, Observable } from "rxjs";
import { Repository } from "../model/repository";
import { Path } from "../constant/path";

@Injectable({
  providedIn: "root",
})
export class RepositoryService {
private http: HttpClient = inject(HttpClient);

  private _searchQuerySubject = new BehaviorSubject<string>("");
  public searchQuery$ = this._searchQuerySubject.asObservable();

  public updateQuery(data: string): void {
    this._searchQuerySubject.next(data);
  }

  public save(data: Repository): Observable<any | null> {
    return this.http.post<Repository>(Path.Repositories, data);
  }

  public GetAllRepositoriesForCurrentUser(): Observable<Repository[]> {
    return this.http.get<Repository[]>(`${Path.Repositories}/all`);
  }

  public GetAllVisibleRepositoriesForUser(username: string): Observable<Repository[]> {
    return this.http.get<Repository[]>(`${Path.Repositories}/all/${username}`);
  }
}