import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DesignRoom } from './design-room';

describe('DesignRoom', () => {
  let component: DesignRoom;
  let fixture: ComponentFixture<DesignRoom>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DesignRoom]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DesignRoom);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
