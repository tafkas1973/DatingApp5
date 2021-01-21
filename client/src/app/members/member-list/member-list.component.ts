import { Component, OnInit } from '@angular/core';
import { Subject } from 'rxjs';
import { Pagination } from 'src/app/_models/pagination';

import { Member } from '../../_models/member';
import { MembersService } from '../../_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  members: Array<Member>;
  pagination: Pagination;
  pageNumber = 1;
  pageSize = 5;

  constructor(private memberService: MembersService) { }
  private notifier = new Subject();

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers() {
    this.memberService
      .getMembers(this.pageNumber, this.pageSize)
      .subscribe(response => {
        this.members = response.result;
        this.pagination = response.pagination;
      });
  }

  pageChanged(event: any) {
    this.pageNumber = event.page;
    this.loadMembers();
  }
}
