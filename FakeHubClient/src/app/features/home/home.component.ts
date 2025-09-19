import { Component, inject, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { DockerImageComponent } from "../../shared/components/docker-image/docker-image.component";
import { Repository, RepositoryOwnedBy } from "../../core/model/repository";
import { RepositoryService } from "../../core/services/repository.service";
import { Router } from "@angular/router";
import { Subscription } from "rxjs";
import { UserBadge } from "../../core/model/user";
import { FormsModule } from "@angular/forms";

@Component({
  selector: "app-home",
  standalone: true,
  imports: [CommonModule, DockerImageComponent, FormsModule],
  templateUrl: "./home.component.html",
  styleUrl: "./home.component.css",
})
export class HomeComponent implements OnInit {
  private repositoryService: RepositoryService = inject(RepositoryService);
  private readonly router: Router = inject(Router);
  private searchSubscription: Subscription | null = null;

  public isTrustedContentVisible: boolean = true;
  public publicRepositories: Repository[] = [];
  public repositories: Repository[] = [];
  public filteredRepositories: Repository[] = [];
  public selectedBadge: UserBadge | null = null;
  public UserBadge = UserBadge;

  public previousQuery: string = "";
  public badgeQuery: string = "";
  public searchQuery: string = "";
  public selectedSortOption: number = 1;
  public sortBy: { id: number; name: string }[] = [
    { id: 1, name: "A-Z" },
    { id: 2, name: "Z-A" },
    { id: 3, name: "Newest" },
    { id: 4, name: "Oldest" },
  ];

  ngOnInit(): void {
    this.searchSubscription = this.repositoryService.searchQuery$.subscribe(
      (query) => {
        this.getFilteredRepositories(query);
      }
    );
  }

  public toggleTrustedContentVisibility() {
    this.isTrustedContentVisible = !this.isTrustedContentVisible;
  }

  public navigateToRepository(id: number | undefined) {
    if (id) this.router.navigate(["/repository/", id]);
  }

  public ngOnDestroy(): void {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
  }

  public filterByBadge(badge: UserBadge) {
    if (this.selectedBadge === badge) {
      this.selectedBadge = null;
      this.badgeQuery = this.removeBadgeFromQuery(this.badgeQuery);
    } else {
      this.selectedBadge = badge;
      this.badgeQuery = this.setBadgeInQuery(this.badgeQuery, badge);
    }
    this.getFilteredRepositories(this.searchQuery);
  }

  public isActive(badge: UserBadge): boolean {
    return this.selectedBadge === badge;
  }

  public applyFilters(): void {
    let temp = [...this.publicRepositories];

    if (this.selectedBadge !== null) {
      temp = temp.filter((r) => {
        if (r.ownedBy === RepositoryOwnedBy.ORGANIZATION) {
          return false;
        }

        return r.badge === this.selectedBadge;
      });
    }

    this.filteredRepositories = temp;
    this.sortRepositories();
  }

  public sortRepositories(): void {
    this.repositories = structuredClone(this.filteredRepositories || []);

    switch (this.selectedSortOption.toString()) {
      case "1": // A-Z
        this.repositories.sort((a, b) => a.fullName.localeCompare(b.fullName));
        break;
      case "2": // Z-A
        this.repositories.sort((a, b) => b.fullName.localeCompare(a.fullName));
        break;
      case "3": // Newest
        this.repositories.sort(
          (a, b) =>
            new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime()
        );
        break;
      case "4": // Oldest
        this.repositories.sort(
          (a, b) =>
            new Date(a.updatedAt).getTime() - new Date(b.updatedAt).getTime()
        );
        break;
    }
  }

  private loadRepositories(query: string | null) {
    this.repositoryService
      .getAllPublicRepositories(query ?? undefined)
      .subscribe((repos) => {
        this.publicRepositories = repos;
        this.applyFilters();
      });
  }

  private parseBadgeFromQuery(query: string | null): UserBadge | null {
    if (!query) return null;

    const badgeTerm = query
      .split(" ")
      .find((t) => t.toLowerCase().startsWith("badge:"));

    if (!badgeTerm) return null;

    const value = badgeTerm.substring(6).toLowerCase();

    const badge = (Object.values(UserBadge) as Array<string | number>)
      .filter((v) => typeof v === "number")
      .map((v) => v as UserBadge)
      .find((b) => UserBadge[b].toLowerCase().includes(value));

    return badge ?? null;
  }

  private removeBadgeFromQuery(query: string): string {
    return (query || "")
      .split(" ")
      .filter((t) => !t.toLowerCase().startsWith("badge:"))
      .join(" ");
  }

  private setBadgeInQuery(query: string, badge: UserBadge): string {
    let q = this.removeBadgeFromQuery(query);
    return `${q} badge:${UserBadge[badge]}`.trim();
  }

  private getFilteredRepositories = (query: string | null) => {
    var badge = this.parseBadgeFromQuery(query);
    if (this.previousQuery?.includes("badge:") && !query?.includes("badge:")) {
      this.badgeQuery = this.removeBadgeFromQuery(this.badgeQuery);
      this.selectedBadge = null;
    } else {
      if (badge != null) {
        this.badgeQuery = this.setBadgeInQuery(this.badgeQuery, badge);
        this.selectedBadge = badge;
        query = this.removeBadgeFromQuery(query ?? "");
      }
    }

    this.previousQuery = query || "";
    this.searchQuery = query || "";

    this.loadRepositories(`${query} ${this.badgeQuery}`.trim());
  };
}
