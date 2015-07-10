﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Glyssen.Character
{
	public class CharacterGroup
	{
		private bool m_isActorAssigned;
		private VoiceActor.VoiceActor m_actorAssigned;

		//For Serialization
		public CharacterGroup()
		{
			CharacterIds = new HashSet<string>();
		}

		public CharacterGroup(int groupNumber)
		{
			CharacterIds = new HashSet<string>();
			GroupNumber = groupNumber;
		}

		public void AssignVoiceActor(VoiceActor.VoiceActor actor)
		{
			if (actor == null)
			{
				return;
			}

			m_isActorAssigned = true;
			m_actorAssigned = actor;
		}

		public void RemoveVoiceActor()
		{
			m_isActorAssigned = false;
			m_actorAssigned = null;
		}

		[XmlElement]
		public int GroupNumber { get; set; }

		[XmlArray("CharacterIds")]
		[XmlArrayItem("CharacterId")]
		[Browsable(false)]
		public HashSet<string> CharacterIds { get; set; }
	
		[XmlIgnore]
		public string CharactersString
		{
			get { return string.Join("; ", CharacterIds); }
		}

		[XmlIgnore]
		public string RequiredAttributes
		{
			get { return ""; }
		}

		[XmlElement]
		public double EstimatedHours { get; set; }

		[XmlElement]
		[Browsable(false)]
		public int VoiceActorAssignedId
		{
			get { return m_actorAssigned == null ? -1 : m_actorAssigned.Id; }
			set
			{
				m_actorAssigned = new VoiceActor.VoiceActor();
				m_actorAssigned.Id = value;
				m_isActorAssigned = true;
				if (value < 0)
				{
					m_isActorAssigned = false;
				}
			}
		}

		[XmlIgnore]
		public string VoiceActorAssignedName
		{
			get { return m_isActorAssigned ? m_actorAssigned.Name : ""; }
		}
	}
}
