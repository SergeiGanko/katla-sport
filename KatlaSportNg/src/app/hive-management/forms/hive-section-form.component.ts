import { ActivatedRoute, Router } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { HiveSection } from '../models/hive-section';
import { HiveSectionService } from '../services/hive-section.service';
import { HiveListItem } from '../models/hive-list-item';
import { HiveService } from '../services/hive.service';

@Component({
  selector: 'app-hive-section-form',
  templateUrl: './hive-section-form.component.html',
  styleUrls: ['./hive-section-form.component.css']
})
export class HiveSectionFormComponent implements OnInit {

  hiveSection = new HiveSection(0, "", "", false, "", 0);
  existed = false;
  hiveId: number;
  hives: HiveListItem[];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private hiveSectionService: HiveSectionService,
    private hiveService: HiveService
  ) { }

  ngOnInit() {
    this.hiveService.getHives().subscribe(h => this.hives = h)
    this.route.params.subscribe(p => {
      if (p['id'] === undefined) return;
      this.hiveSectionService.getHiveSection(p['id']).subscribe(hs => this.hiveSection = hs);
      this.hiveId = p['hiveId'];
      this.existed = true;
    });
  }

  navigateToHiveSections() {
    if (this.hiveId === undefined) {
      this.router.navigate(['/hives']);
    } 
    else {
      this.router.navigate([`/hive/${this.hiveId}/sections`]);
    }
  }

  onCancel() {
    this.navigateToHiveSections();
  }

  onSubmit() {
    if (this.existed) {
      this.hiveSection.hiveId = this.hiveId;
      this.hiveSectionService.updateHiveSection(this.hiveSection)
        .subscribe(c => { this.navigateToHiveSections(); } );
      
    } else {
      this.hiveSection.hiveId = this.hiveId;
      this.hiveSectionService.addHiveSection(this.hiveSection)
        .subscribe(c => { this.navigateToHiveSections(); });
    }
  }

  onDelete() {
    this.hiveSectionService.setHiveSectionStatus(this.hiveSection.id, true)
      .subscribe(c => this.hiveSection.isDelited = true);
  }

  onUndelete() {
    this.hiveSectionService.setHiveSectionStatus(this.hiveSection.id, false)
      .subscribe(c => this.hiveSection.isDelited = false);
  }

  onPurge() {
    this.hiveSectionService.deleteHiveSection(this.hiveSection.id)
      .subscribe(c => this.navigateToHiveSections());
  }
}
