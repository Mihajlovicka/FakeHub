import { Component, inject, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { DockerImageComponent } from "../../shared/components/docker-image/docker-image.component";
import { Repository, RepositoryOwnedBy } from "../../core/model/repository";
import { RepositoryService } from "../../core/services/repository.service";
import { Router } from "@angular/router";
import { Subscription } from "rxjs";
import { UserBadge } from "../../core/model/user";
import { FormsModule } from "@angular/forms";
import { MatCheckboxModule } from "@angular/material/checkbox";

@Component({
  selector: "app-home",
  standalone: true,
  imports: [CommonModule, DockerImageComponent, FormsModule, MatCheckboxModule],
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
  public selectedBadges: UserBadge[] = [];
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

  public trustedOptions = [
  { label: "Docker Official Image", value: UserBadge.DockerOfficialImage },
  { label: "Verified Publisher", value: UserBadge.VerifiedPublisher },
  { label: "Sponsored OSS", value: UserBadge.SponsoredOSS },
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
    const index = this.selectedBadges.indexOf(badge);
    if (index > -1) {
      this.selectedBadges.splice(index, 1);
    } else {
      this.selectedBadges.push(badge);
    }
    this.applyFilters();
  }

  public isActive(badge: UserBadge): boolean {
    return this.selectedBadges.includes(badge);
  }

  public applyFilters(): void {
    let temp = [...this.publicRepositories];

  if (this.selectedBadges.length > 0) {
    temp = temp.filter((r) => {
      if (r.ownedBy === RepositoryOwnedBy.ORGANIZATION) {
        return false;
      }
      return this.selectedBadges.includes(r.badge);
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

  private parseBadgeFromQuery(query: string | null): UserBadge[] {
    if (!query) return [];

    const badgeTerms = query
    .split(" ")
    .filter((t) => t.toLowerCase().startsWith("badge:"))
    .map((t) => t.substring(6).toLowerCase());

  const badges = (Object.values(UserBadge) as Array<string | number>)
    .filter((v) => typeof v === "number")
    .map((v) => v as UserBadge)
    .filter((b) =>
      badgeTerms.some((term) => UserBadge[b].toLowerCase().includes(term))
    );

    return badges;
  }

  private removeBadgeFromQuery(query: string): string {
    return (query || "")
      .split(" ")
      .filter((t) => !t.toLowerCase().startsWith("badge:"))
      .join(" ");
  }

  private getFilteredRepositories = (query: string | null) => {
    var badges = this.parseBadgeFromQuery(query);
    if (this.previousQuery?.includes("badge:") && !query?.includes("badge:")) {
    this.badgeQuery = "";
      this.selectedBadges = [];
    } else if (badges.length > 0) {
      this.selectedBadges = badges;
      query = this.removeBadgeFromQuery(query ?? "");
    }

    this.previousQuery = query || "";
    this.searchQuery = query || "";

    this.loadRepositories(`${query} ${this.badgeQuery}`.trim());
  };
}
