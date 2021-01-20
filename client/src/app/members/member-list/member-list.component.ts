import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { Member } from '../../_models/member';
import { MembersService } from '../../_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit, OnDestroy {
  members: Array<Member>;

  constructor(private memberService: MembersService) { }
  private notifier = new Subject();

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers() {
    this.memberService
      .getMembers()
      .pipe(takeUntil(this.notifier))
      .subscribe(members => {
        this.members = members;
      })
  }

  ngOnDestroy() {
    this.notifier.next();
    this.notifier.complete();
  }
}
