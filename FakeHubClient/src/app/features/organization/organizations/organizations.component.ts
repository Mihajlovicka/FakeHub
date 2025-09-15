import {Component, inject, OnDestroy, OnInit} from "@angular/core";
import { Organization } from "../../../core/model/organization";
import { Router, RouterLink } from "@angular/router";
import { OrganizationService } from "../../../core/services/organization.service";
import {Subscription} from "rxjs";

@Component({
  selector: "app-organizations",
  standalone: true,
  imports: [RouterLink],
  templateUrl: "./organizations.component.html",
  styleUrl: "./organizations.component.css",
})
export class OrganizationsComponent implements OnInit, OnDestroy {
  public organizations: Organization[] = [];
  public readonly router: Router = inject(Router);
  public readonly organizationService: OrganizationService =
    inject(OrganizationService);

  private searchSubscription: Subscription | null = null;

  public addOrganization(): void {
    this.router.navigate(["/organization/add"]);
  }

  public ngOnInit(): void {
    this.searchSubscription = this.organizationService.searchQuery$.subscribe(
        query => {
          this.organizationService.getOrganizations(query).subscribe(
              organizations => {
                this.organizations = organizations;
              }
          );
        }
    );
  }

  public ngOnDestroy(): void {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
  }
}
