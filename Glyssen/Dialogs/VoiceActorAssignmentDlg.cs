﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Character;
using Glyssen.Controls;
using Glyssen.Properties;
using Glyssen.Rules;
using L10NSharp;
using SIL.Extensions;
using SIL.IO;
using SIL.ObjectModel;

namespace Glyssen.Dialogs
{
	public partial class VoiceActorAssignmentDlg : Form
	{
		private readonly Project m_project;
		private bool m_canAssign;
		private bool m_switchToCellSelectOnMouseUp;
		private DataGridViewCellStyle m_wordWrapCellStyle;

		private enum DragSource
		{
			CharacterGroupGrid,
			VoiceActorGrid
		};
		private DragSource m_dragSource;  

		public VoiceActorAssignmentDlg(Project project)
		{
			InitializeComponent();

			m_wordWrapCellStyle = new DataGridViewCellStyle();
			m_wordWrapCellStyle.WrapMode = DataGridViewTriState.True;

			m_characterGroupGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

			WindowState = FormWindowState.Maximized;

			AlignBtnAssignActorToSplitter();

			m_project = project;
			m_canAssign = true;

			SortableBindingList<CharacterGroup> characterGroups;
			if (m_project.CharacterGroupList.CharacterGroups.Any())
				characterGroups = new SortableBindingList<CharacterGroup>(m_project.CharacterGroupList.CharacterGroups);
			else
				characterGroups = CreateInitialGroupsFromTemplate();

#if DEBUG
			var p = new Proximity(m_project);
			foreach (var group in characterGroups.OrderBy(g => g.GroupNumber))
				Debug.WriteLine(group.GroupNumber + ": " + p.CalculateMinimumProximity(group.CharacterIds));
#endif
			
			m_project.CharacterGroupList.PopulateAttributesDisplay();

			m_characterGroupGrid.DataSource = characterGroups;
			m_characterGroupGrid.MultiSelect = true;
			m_characterGroupGrid.Sort(m_characterGroupGrid.Columns["GroupNumber"], ListSortDirection.Ascending);

			m_voiceActorGrid.CharacterGroupsWithAssignedActors = characterGroups;
			m_voiceActorGrid.Initialize(m_project);
			m_voiceActorGrid.ReadOnly = true;
			m_voiceActorGrid.Saved += m_voiceActorGrid_Saved;
			m_voiceActorGrid.CellUpdated += m_voiceActorGrid_CellUpdated;
			m_voiceActorGrid.CellDoubleClicked += m_voiceActorGrid_CellDoubleClicked;
			m_voiceActorGrid.GridMouseMove += m_voiceActorGrid_MouseMove;
			m_voiceActorGrid.UserRemovedRows += m_voiceActorGrid_UserRemovedRows;
			m_voiceActorGrid.SelectionChanged += m_eitherGrid_SelectionChanged;
		}

		private SortableBindingList<CharacterGroup> CreateInitialGroupsFromTemplate()
		{
			var characterGroups = new SortableBindingList<CharacterGroup>(m_project.CharacterGroupList.CharacterGroups);

			CharacterGroupTemplate charGroupTemplate;
			using (TempFile tempFile = new TempFile())
			{
				File.WriteAllBytes(tempFile.Path, Resources.CharacterGroups);
				ICharacterGroupSource charGroupSource = new CharacterGroupTemplateExcelFile(tempFile.Path);
				charGroupTemplate = charGroupSource.GetTemplate(m_project.VoiceActorList.Actors.Count);
			}

			var characterIdToCharacterGroup = new Dictionary<string, CharacterGroup>();
			ISet<string> matchedCharacterIds = new HashSet<string>();
			ISet<CharacterDetail> standardCharacterDetails = new HashSet<CharacterDetail>();

			foreach (CharacterGroup group in charGroupTemplate.CharacterGroups.Values)
			{
				group.CharacterIds.IntersectWith(m_project.IncludedCharacterIds);

				if (!group.CharacterIds.Any())
					continue;

				characterGroups.Add(group);

				ProcessCharacterIds(group, characterIdToCharacterGroup, standardCharacterDetails);
				matchedCharacterIds.AddRange(group.CharacterIds);
			}

			// Add an extra group for any characters which weren't in the template
			var unmatchedCharacters = m_project.IncludedCharacterIds.Except(matchedCharacterIds);
			var unmatchedCharacterGroup = new CharacterGroup { GroupNumber = 999, CharacterIds = new CharacterIdHashSet(unmatchedCharacters) };
			characterGroups.Add(unmatchedCharacterGroup);
			ProcessCharacterIds(unmatchedCharacterGroup, characterIdToCharacterGroup, standardCharacterDetails);

			foreach (var detail in CharacterDetailData.Singleton.GetAll().Union(standardCharacterDetails))
			{
				if (characterIdToCharacterGroup.ContainsKey(detail.Character))
				{
					var group = characterIdToCharacterGroup[detail.Character];

					if (!string.IsNullOrWhiteSpace(detail.Gender))
						group.GenderAttributes.Add(detail.Gender);
					if (!string.IsNullOrWhiteSpace(detail.Age))
						group.AgeAttributes.Add(detail.Age);
					if (detail.Status)
						group.Status = true;
				}
			}

			m_project.CharacterGroupList.PopulateEstimatedHours(m_project.IncludedBooks);

			return characterGroups;
		}

		private void ProcessCharacterIds(CharacterGroup group, Dictionary<string, CharacterGroup> characterIdToCharacterGroup, ISet<CharacterDetail> standardCharacterDetails)
		{
			foreach (string characterId in group.CharacterIds)
			{
				if (!characterIdToCharacterGroup.ContainsKey(characterId))
					characterIdToCharacterGroup.Add(characterId, group);
				if (CharacterVerseData.IsCharacterOfType(characterId, CharacterVerseData.StandardCharacter.Narrator))
					standardCharacterDetails.Add(new CharacterDetail { Character = characterId, Gender = "Either", Age = "Middle Adult", Status = true });
				else if (CharacterVerseData.IsCharacterStandard(characterId, false))
					standardCharacterDetails.Add(new CharacterDetail { Character = characterId, Gender = "Either", Age = "Middle Adult" });
			}
		}

		private void SaveAssignments()
		{
			m_project.SaveCharacterGroupData();
			m_saveStatus.OnSaved();
		}

		private void AssignSelectedActorToSelectedGroup()
		{
			if (!m_canAssign)
				return;

			CharacterGroup group = m_characterGroupGrid.SelectedRows[0].DataBoundItem as CharacterGroup;
			if (group == null)
				return;
			group.AssignVoiceActor(m_voiceActorGrid.SelectedVoiceActorEntity);

			SaveAssignments();

			m_characterGroupGrid.Refresh();
			m_voiceActorGrid.RefreshSort();
		}

		private void UnAssignActorsFromSelectedGroups()
		{
			bool multipleGroupsSelected = m_characterGroupGrid.SelectedRows.Count > 1;

			string dlgMessage = multipleGroupsSelected
				? LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.DeleteGroupsDialog.MessagePlural",
					"Are you sure you want to un-assign the actors from the selected groups?")
				: LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.DeleteGroupsDialog.MessageSingular",
					"Are you sure you want to un-assign the actor from the selected group?");
			string dlgTitle = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.DeleteGroupsDialog.Title", "Confirm");
			if (MessageBox.Show(dlgMessage, dlgTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				for (int i = 0; i < m_characterGroupGrid.SelectedRows.Count; i++)
				{
					CharacterGroup entry = m_characterGroupGrid.SelectedRows[i].DataBoundItem as CharacterGroup;
					entry.RemoveVoiceActor();
				}

				SaveAssignments();

				m_characterGroupGrid.Refresh();
				m_voiceActorGrid.RefreshSort();
			}			
		}

		private void AlignBtnAssignActorToSplitter()
		{
			int xDist = splitContainer1.SplitterDistance;
			m_btnAssignActor.Location = new Point(xDist + 19, m_btnAssignActor.Location.Y);
		}

		private void m_voiceActorGrid_EnterEditingMode()
		{
			m_voiceActorGrid.ReadOnly = false;
			//Restore EditMode overwritten in m_voiceActorGrid_Leave
			m_voiceActorGrid.EditMode = DataGridViewEditMode.EditOnEnter;
			m_voiceActorGrid.Focus();			
		}

		private void m_voiceActorGrid_ExitEditingMode()
		{
			m_voiceActorGrid.ReadOnly = true;
			//EndEdit is insufficient in and of itself
			m_voiceActorGrid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
			m_voiceActorGrid.EndEdit();
		}

		private void m_btnAssignActor_Click(object sender, EventArgs e)
		{
			AssignSelectedActorToSelectedGroup();
		}

		private void m_voiceActorGrid_Saved(object sender, EventArgs e)
		{
			m_saveStatus.OnSaved();
		}

		private void m_voiceActorGrid_CellUpdated(object sender, DataGridViewCellEventArgs e)
		{
			var grid = sender as DataGridView;
			if (grid.Columns[e.ColumnIndex].DataPropertyName == "Name")
				m_characterGroupGrid.Refresh();
		}

		private void m_characterGroupGrid_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			var grid = sender as DataGridView;

			if (grid.Columns[e.ColumnIndex].DataPropertyName == "VoiceActorAssignedName" && e.RowIndex >= 0 && e.Button == MouseButtons.Left)
				AssignSelectedActorToSelectedGroup();
		}

		private void m_voiceActorGrid_CellDoubleClicked(object sender, DataGridViewCellMouseEventArgs e)
		{
			var grid = sender as DataGridView;

			if (!grid.ReadOnly)
				return;

			if (grid.Columns[e.ColumnIndex].DataPropertyName == "Name" && e.RowIndex >= 0 && e.Button == MouseButtons.Left)
			{
				if (grid.Rows[e.RowIndex].IsNewRow)
				{
					m_voiceActorGrid_EnterEditingMode();
				}
				else
				{
					AssignSelectedActorToSelectedGroup();
				}
			}
		}

		private void m_voiceActorGrid_UserRemovedRows(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			m_characterGroupGrid.Refresh();
		}

		private void m_eitherGrid_SelectionChanged(object sender, EventArgs e)
		{
			m_canAssign = m_voiceActorGrid.SelectedRows.Count == 1 && m_characterGroupGrid.SelectedRows.Count == 1;

			m_btnAssignActor.Enabled = m_canAssign;
		}

		private void m_characterGroupGrid_SelectionChanged(object sender, EventArgs e)
		{
			m_eitherGrid_SelectionChanged(sender, e);
		}

		private void m_characterGroupGrid_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Delete)
			{
				UnAssignActorsFromSelectedGroups();
			}
		}

		private void m_btnExport_Click(object sender, EventArgs e)
		{
			var characterGroups = m_characterGroupGrid.DataSource as SortableBindingList<CharacterGroup>;

			bool assignmentsComplete = characterGroups.All(t => t.IsVoiceActorAssigned);

			string dlgMessage = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ExportIncompleteScript.Message", "Some of the character groups have no voice talent assigned. Are you sure you want to export an incomplete script?\n(Note: You can export the script again as many times as you want.)");
			string dlgTitle = LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ExportIncompleteScript.Title", "Export Incomplete Script?");
			if (assignmentsComplete || MessageBox.Show(dlgMessage, dlgTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				SaveAssignments();
				new ProjectExport(m_project).Export(this);
			}
		}

		private void m_voiceActorGrid_MouseMove(object sender, MouseEventArgs e)
		{
			if (m_voiceActorGrid.ReadOnly && e.Button == MouseButtons.Left)
			{
				var hitInfo = m_voiceActorGrid.HitTest(e.X, e.Y);
				if (hitInfo.Type == DataGridViewHitTestType.Cell)
				{
					var sourceActor = m_voiceActorGrid.SelectedVoiceActorEntity;
					if (sourceActor != null && sourceActor.HasMeaningfulData())
					{
						m_dragSource = DragSource.VoiceActorGrid;
						DoDragDrop(sourceActor, DragDropEffects.Copy);
					}
					else
					{
						DoDragDrop(0, DragDropEffects.None);
					}
				}
			}
		}

		private void m_characterGroupGrid_DragOver(object sender, DragEventArgs e)
		{
			if (!(e.Data.GetDataPresent(typeof(VoiceActor.VoiceActor)) || e.Data.GetDataPresent(typeof(CharacterGroup))))
			{
				e.Effect = DragDropEffects.None;
				return;
			}
			e.Effect = DragDropEffects.Copy;
		}

		private void m_characterGroupGrid_DragDrop(object sender, DragEventArgs e)
		{
			Point p = m_characterGroupGrid.PointToClient(new Point(e.X, e.Y));
			var hitInfo = m_characterGroupGrid.HitTest(p.X, p.Y);
			if (hitInfo.Type == DataGridViewHitTestType.Cell)
			{
				if (m_dragSource == DragSource.VoiceActorGrid)
				{
					m_characterGroupGrid.ClearSelection();
					m_characterGroupGrid.Rows[hitInfo.RowIndex].Selected = true;
					AssignSelectedActorToSelectedGroup();
				}
				else if (m_dragSource == DragSource.CharacterGroupGrid)
				{
					CharacterGroup sourceGroup = m_characterGroupGrid.SelectedRows[0].DataBoundItem as CharacterGroup;
					CharacterGroup destinationGroup = m_characterGroupGrid.Rows[hitInfo.RowIndex].DataBoundItem as CharacterGroup;

					VoiceActor.VoiceActor sourceActor = sourceGroup.VoiceActorAssigned;
					VoiceActor.VoiceActor destinationActor = destinationGroup.VoiceActorAssigned;

					destinationGroup.AssignVoiceActor(sourceActor);
					if (destinationActor != null)
						sourceGroup.AssignVoiceActor(destinationActor);
					else
						sourceGroup.RemoveVoiceActor();

					m_characterGroupGrid.ClearSelection();
					m_characterGroupGrid.Rows[hitInfo.RowIndex].Selected = true;

					SaveAssignments();
					m_characterGroupGrid.Refresh();				
				}
			}
		}

		private void VoiceActorAssignmentDlg_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.F6)
			{
				if (m_characterGroupGrid.ContainsFocus)
					m_voiceActorGrid.Focus();
				else
					m_characterGroupGrid.Focus();
			}
		}

		private void m_characterGroupGrid_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				var hitInfo = m_characterGroupGrid.HitTest(e.X, e.Y);
				if (hitInfo.Type == DataGridViewHitTestType.Cell && m_characterGroupGrid.Columns[hitInfo.ColumnIndex].DataPropertyName == "VoiceActorAssignedName")
				{
					CharacterGroup sourceGroup = m_characterGroupGrid.SelectedRows[0].DataBoundItem as CharacterGroup;
					if (sourceGroup.IsVoiceActorAssigned)
					{
						m_dragSource = DragSource.CharacterGroupGrid;
						DoDragDrop(sourceGroup, DragDropEffects.Copy);
					}
					else
					{
						DoDragDrop(0, DragDropEffects.None);
					}
				}
			}
		}

		private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
		{
			AlignBtnAssignActorToSplitter();
		}

		private void splitContainer1_MouseUp(object sender, MouseEventArgs e)
		{
			//A disabled control is un-focusable
			bool btnIsEnabled = m_btnAssignActor.Enabled;
			m_btnAssignActor.Enabled = true;

			//Focus on Assign button to remove awkward focus rectangle around splitter
			m_btnAssignActor.Focus();

			m_btnAssignActor.Enabled = btnIsEnabled;
		}

		private void m_linkClose_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			SaveAssignments();
			Close();
		}

		private void m_linkEdit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			m_voiceActorGrid_EnterEditingMode();
		}

		private void m_voiceActorGrid_Leave(object sender, EventArgs e)
		{
			m_voiceActorGrid_ExitEditingMode();
		}

		private void m_helpIcon_MouseClick(object sender, MouseEventArgs e)
		{
			var pic = sender as PictureBox;
			m_toolTip.Show("In this window, you can assign voice actors to speak the parts of a group of Biblical characters.\n" +
							"To assign an actor to a group, use one of the assignment methods below:" +
							"\n1) Select a group and an actor and click \"Assign\"" +
							"\n2) Select a group and an actor and double-click the character group or voice actor's name" +
							"\n3) Drag an actor's name into the corresponding \"Actor Assigned\" column of a character group",
				pic, new Point(e.X + 10, e.Y + 5));
		}

		private void m_helpIcon_MouseLeave(object sender, EventArgs e)
		{
			m_toolTip.Hide(this);
		}

		private void m_assignActorToGroupToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AssignSelectedActorToSelectedGroup();
		}

		private void m_unAssignActorFromGroupToolStripMenuItem_Click(object sender, EventArgs e)
		{
			UnAssignActorsFromSelectedGroups();
		}

		private void m_splitGroupToolStripMenuItem_Click(object sender, EventArgs e)
		{
			//Todo
		}

		private void m_contextMenuCharacterGroups_Opening(object sender, CancelEventArgs e)
		{
			bool multipleGroupsSelected = m_characterGroupGrid.SelectedRows.Count > 1;
			bool multipleActorsSelected = m_voiceActorGrid.SelectedRows.Count > 1;

			m_assignActorToGroupToolStripMenuItem.Enabled = !multipleGroupsSelected && !multipleActorsSelected;

			m_unAssignActorFromGroupToolStripMenuItem.Text = multipleGroupsSelected
				? LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ContextMenus.UnassignMultipleGroups",
					"Un-Assign Actors from Selected Groups")
				: LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ContextMenus.UnassignSingleGroup",
					"Un-Assign Actor from Selected Group");

			m_splitGroupToolStripMenuItem.Enabled = !multipleGroupsSelected;
		}

		private void m_editActorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			m_voiceActorGrid_EnterEditingMode();
		}

		private void m_deleteActorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SendKeys.Send("{DEL}");
		}

		private void m_contextMenuVoiceActors_Opening(object sender, CancelEventArgs e)
		{
			bool multipleGroupsSelected = m_characterGroupGrid.SelectedRows.Count > 1;
			bool multipleActorsSelected = m_voiceActorGrid.SelectedRows.Count > 1;

			m_assignActorToGroupToolStripMenuItem2.Enabled = !multipleActorsSelected && !multipleGroupsSelected;
			m_editActorToolStripMenuItem.Enabled = !multipleActorsSelected && !multipleGroupsSelected;

			m_deleteActorToolStripMenuItem.Text = multipleActorsSelected
				? LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ContextMenus.DeleteMultipleActors",
					"Delete Selected Actors")
				: LocalizationManager.GetString("DialogBoxes.VoiceActorAssignmentDlg.ContextMenus.DeleteSingleActor",
					"Delete Selected Actor");
		}

		private void m_characterGroupGrid_CellLeave(object sender, DataGridViewCellEventArgs e)
		{
			if (m_characterGroupGrid.Columns[e.ColumnIndex].DataPropertyName == "CharacterIds")
			{
				m_characterGroupGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				m_characterGroupGrid.Rows[e.RowIndex].Height = 22;
				m_characterGroupGrid.Rows[e.RowIndex].DefaultCellStyle = null;
				m_characterGroupGrid.ReadOnly = true;
				m_characterGroupGrid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
				m_characterGroupGrid.ClearSelection();
				m_characterGroupGrid.Rows[e.RowIndex].Selected = true;
			}
		}

		private void m_characterGroupGrid_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && m_characterGroupGrid.Columns[e.ColumnIndex].DataPropertyName == "CharacterIds")
			{
				//By default, the whole row remains selected when the cell is first clicked
				m_switchToCellSelectOnMouseUp = true;

				m_characterGroupGrid.CurrentCell = m_characterGroupGrid[e.ColumnIndex, e.RowIndex];

				var data = m_characterGroupGrid.CurrentCell.Value as HashSet<string>;
				int estimatedRowHeight = data.Count*21;
				int maxRowHeight = 200;

				//Without the +1, an extra row is drawn, and the list starts scrolled down one item
				m_characterGroupGrid.Rows[e.RowIndex].Height = Math.Max(21, Math.Min(estimatedRowHeight, maxRowHeight)) + 1;
				m_characterGroupGrid.Rows[e.RowIndex].DefaultCellStyle = m_wordWrapCellStyle;

				//Scroll table if expanded row will be hidden
				int dRows = e.RowIndex - m_characterGroupGrid.FirstDisplayedScrollingRowIndex;
				if (dRows*22 + maxRowHeight >= m_characterGroupGrid.Height - m_characterGroupGrid.ColumnHeadersHeight)
				{
					m_characterGroupGrid.FirstDisplayedScrollingRowIndex = e.RowIndex - 5;
				}

				m_characterGroupGrid.ReadOnly = false;
				m_characterGroupGrid.EditMode = DataGridViewEditMode.EditOnEnter;

				m_characterGroupGrid.MultiSelect = false;
			}			
		}

		private void m_characterGroupGrid_MouseUp(object sender, MouseEventArgs e)
		{
			if (m_switchToCellSelectOnMouseUp)
			{
				m_characterGroupGrid.SelectionMode = DataGridViewSelectionMode.CellSelect;
				m_characterGroupGrid.MultiSelect = true;
				m_switchToCellSelectOnMouseUp = false;
			}
		}
	}
}
