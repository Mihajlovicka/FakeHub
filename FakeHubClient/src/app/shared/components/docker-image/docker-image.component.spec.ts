import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DockerImageComponent } from './docker-image.component';

describe('DockerImageComponent', () => {
  let component: DockerImageComponent;
  let fixture: ComponentFixture<DockerImageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DockerImageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DockerImageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
