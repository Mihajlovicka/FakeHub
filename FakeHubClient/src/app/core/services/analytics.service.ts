import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { Path } from "../constant/path";

@Injectable({
  providedIn: "root",
})
export class AnalyticsService {
  private http: HttpClient = inject(HttpClient);

  public getElasticLogs(size: number = 100): Observable<any> {
    return this.http.get<any>(Path.ElasticLogs, { params: { size: size.toString() } });
  }


  public searchElasticLogs(
    query: string,
    level: string,
    from: string,
    to: string,
    size: number = 100
  ): Observable<any> {
    const params: any = { query, size: size.toString() };
    if (level) params.level = level;
    if (from) params.from = from;
    if (to) params.to = to;
    return this.http.get<any>('api/elasticsearch/search', { params });
  }
}