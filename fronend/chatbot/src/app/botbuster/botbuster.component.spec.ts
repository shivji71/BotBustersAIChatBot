import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { BotbusterComponent } from './botbuster.component';

describe('BotbusterComponent', () => {
  let component: BotbusterComponent;
  let fixture: ComponentFixture<BotbusterComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ BotbusterComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BotbusterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
