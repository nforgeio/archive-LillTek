//-----------------------------------------------------------------------------
// FILE:        ClusterMemberSettings.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright (c) 2005-2015 by Jeffrey Lill.  All rights reserved.
// DESCRIPTION: Defines the settings used when creating a ClusterMember instance.

using System;

using LillTek.Common;

namespace LillTek.Messaging
{
    /// <summary>
    /// Defines the settings used when creating a <see cref="ClusterMember" /> instance.
    /// </summary>
    /// <threadsafety instance="false" />
    public class ClusterMemberSettings
    {
        //---------------------------------------------------------------------
        // Static members

        /// <summary>
        /// Loads and returns a <see cref="ClusterMemberSettings" /> instance 
        /// from the application configuration using the specified configuration 
        /// key prefix.
        /// </summary>
        /// <param name="keyPrefix">The configuration key prefix.</param>
        /// <remarks>
        /// <para>
        /// The <see cref="ClusterMember" /> settings are loaded from the application
        /// configuration, under the specified key prefix.  The following
        /// settings are recognized by the class:
        /// </para>
        /// <div class="tablediv">
        /// <table class="dtTABLE" cellspacing="0" ID="Table1">
        /// <tr valign="top">
        /// <th width="1">Setting</th>        
        /// <th width="1">Default</th>
        /// <th width="90%">Description</th>
        /// </tr>
        /// <tr valign="top">
        ///     <td>ClusterBaseEP</td>
        ///     <td>(required)</td>
        ///     <td>
        ///     The cluster's logical base endpoint.  Instance endpoints will be constructed
        ///     by appending a GUID segment to this and the cluster broadcast endpoint
        ///     will be generated by appending "/*".  This defaults to null and must be
        ///     set explicitly or loaded from the application configuration.  This
        ///     property must be initialized.
        ///     </td>
        /// </tr>
        /// <tr valign="top">
        ///     <td>Mode</td>
        ///     <td>Normal</td>
        ///     <td>
        ///     Specifies the member's startup mode.  The possible values include:
        ///     <list type="table">
        ///         <item>
        ///             <term>Normal</term>
        ///             <description>
        ///             Indicates that a <see cref="ClusterMember" /> should go through 
        ///             the normal master election cycle and eventually enter into the
        ///             <see cref="ClusterMemberState.Master" /> or <see cref="ClusterMemberState.Slave" />
        ///             state.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>Observer</term>
        ///             <description>
        ///             Indicates that a <see cref="ClusterMember" /> should immediately enter the
        ///             <see cref="ClusterMemberState.Observer" /> state and remain there.  Cluster
        ///             observer state is replicated across the cluster so other instances know
        ///             about these instances but observers will never be elected as master.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>Monitor</term>
        ///             <description>
        ///             Indicates that a <see cref="ClusterMember" /> should immediately enter the 
        ///             <see cref="ClusterMemberState.Monitor" /> state and remain there.  Monitors
        ///             collect and maintain cluster status but do not actively participate in the
        ///             cluster.  No member status information about a monitor will be replicated
        ///             across the cluster.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>Slave</term>
        ///             <description>
        ///             Indicates that a <see cref="ClusterMember" /> instance prefers to be
        ///             started as a cluster slave if there's another instance running without
        ///             this preference.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>Master</term>
        ///             <description>
        ///             Indicates that a <see cref="ClusterMember" /> instances prefers to be
        ///             started as the cluster master.  If a master is already running and
        ///             it does not have this preference then a master election will be called.
        ///             </description>
        ///         </item>
        ///     </list>
        ///     </td>
        /// </tr>
        /// <tr valign="top">
        ///     <td>MasterBroadcastInterval</td>
        ///     <td>30s</td>
        ///     <td>
        ///     The interval at which the cluster master should broadcast cluster update
        ///     messages.
        ///     </td>
        /// </tr>
        /// <tr valign="top">
        ///     <td>SlaveUpdateInterval</td>
        ///     <td>30s</td>
        ///     <td>
        ///     The interval at which slaves send their <see cref="ClusterMemberStatus" /> 
        ///     information to the master.
        ///     </td>
        /// </tr>
        /// <summary>
        /// </summary>
        /// <tr valign="top">
        ///     <td>ElectionInterval</td>
        ///     <td>10s</td>
        ///     <td>
        ///     The time period cluster members will wait while collecting <see cref="ClusterMemberStatus" />
        ///     broadcasts from peers before concluding a master election.
        ///     </td>
        /// </tr>
        /// <tr valign="top">
        ///     <td>MissingMasterCount</td>
        ///     <td>2</td>
        ///     <td>
        ///     The number of times a cluster slave member should allow for missed
        ///     master <see cref="ClusterStatus" /> broadcasts
        ///     before calling for a master election.
        ///     </td>
        /// </tr>
        /// <tr valign="top">
        ///     <td>MissingSlaveCount</td>
        ///     <td>2</td>
        ///     <td>
        ///     The number of times the cluster master should allow for missed
        ///     slave <see cref="ClusterMemberStatus" /> transmissions
        ///     before removing the slave from the cluster state.
        ///     </td>
        /// </tr>
        /// <tr valign="top">
        ///     <td>MasterBkInterval</td>
        ///     <td>1s</td>
        ///     <td>
        ///     The interval at which the cluster master instance should raise its
        ///     <see cref="ClusterMember.MasterTask" /> event so that derived classes
        ///     can implement custom background processing behavior.
        ///     </td>
        /// </tr>
        /// <tr valign="top">
        ///     <td>SlaveBkInterval</td>
        ///     <td>1s</td>
        ///     <td>
        ///     The interval at which the cluster slaves should raise their
        ///     <see cref="ClusterMember.SlaveTask" /> event so that derived classes
        ///     can implement custom background processing behavior.
        ///     </td>
        /// </tr>
        /// <tr valign="top">
        ///     <td>BkInterval</td>
        ///     <td>1s</td>
        ///     <td>
        ///     The background task polling interval.  This value should be less than
        ///     or equal to the minimum of <see cref="MasterBroadcastInterval" />,
        ///     <see cref="MasterBkInterval" />, and <see cref="SlaveBkInterval" />.
        ///     </td>
        /// </tr>
        /// </table>
        /// </div>
        /// </remarks>
        /// <returns>The <see cref="ClusterMemberSettings" /> instance.</returns>
        public static ClusterMemberSettings LoadConfig(string keyPrefix)
        {
            return new ClusterMemberSettings(keyPrefix);
        }

        //---------------------------------------------------------------------
        // Instance members

        /// <summary>
        /// The cluster's logical base endpoint.  Instance endpoints will be constructed
        /// by appending a GUID segment to this and the cluster broadcast endpoint
        /// will be formated by appending "/*".  This defaults null and must be
        /// set explicitly or loaded from the application configuration.  This
        /// property must explicitly specified.
        /// </summary>
        public MsgEP ClusterBaseEP = null;

        /// <summary>
        /// Specifies the cluster member's startup mode.  This defaults to
        /// <see cref="ClusterMemberMode.Normal" />.
        /// </summary>
        public ClusterMemberMode Mode = ClusterMemberMode.Normal;

        /// <summary>
        /// The interval at which the cluster master should broadcast cluster update
        /// messages.  This defaults to <b>30s</b>.
        /// </summary>
        public TimeSpan MasterBroadcastInterval = TimeSpan.FromSeconds(30);

        /// <summary>
        /// The interval at which slaves send their <see cref="ClusterMemberStatus" /> 
        /// information to the master.  This defaults to <b>30s</b>.
        /// </summary>
        public TimeSpan SlaveUpdateInterval = TimeSpan.FromSeconds(30);

        /// <summary>
        /// The time period cluster members will wait while collecting <see cref="ClusterMemberStatus" />
        /// broadcasts from peers before concluding the election.  This defaults to <b>5s</b>.
        /// </summary>
        public TimeSpan ElectionInterval = TimeSpan.FromSeconds(10);

        /// <summary>
        /// The number of times a cluster slave member should allow for missed
        /// master <see cref="ClusterStatus" /> broadcasts before calling for a 
        /// master election.  This defaults to <b>2</b>.
        /// </summary>
        public int MissingMasterCount = 2;

        /// <summary>
        /// The number of times a cluster master should allow for a missed
        /// slave <see cref="ClusterMemberStatus" /> transmission
        /// before removing the slave from the cluster state.  This defaults to <b>2</b>.
        /// </summary>
        public int MissingSlaveCount = 2;

        /// <summary>
        /// The interval at which the cluster master instance should raise its
        /// <see cref="ClusterMember.MasterTask" /> event so that derived classes
        /// can implement custom background processing behavior.  This defaults
        /// to <b>1s</b>.
        /// </summary>
        public TimeSpan MasterBkInterval = TimeSpan.FromSeconds(1);

        /// <summary>
        /// The interval at which the cluster slaves should raise their
        /// <see cref="ClusterMember.SlaveTask" /> event so that derived classes
        /// can implement custom background processing behavior.  This defaults
        /// to <b>1s</b>.
        /// </summary>
        public TimeSpan SlaveBkInterval = TimeSpan.FromSeconds(1);

        /// <summary>
        /// The background task polling interval.  This value should be less than
        /// or equal to the minimum of <see cref="MasterBroadcastInterval" />,
        /// <see cref="MasterBkInterval" />, and <see cref="SlaveBkInterval" />.
        /// This defaults to <b>1s</b>.
        /// </summary>
        public TimeSpan BkInterval = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Computes and returns the maximum time a slave instance should wait for
        /// a master to broadcast a <see cref="ClusterStatus" /> before concluding that
        /// there is no master.  This is computed by multiplying <see cref="MasterBroadcastInterval" />
        /// by <see cref="MissingMasterCount" />.
        /// </summary>
        public TimeSpan MissingMasterInterval
        {
            get { return TimeSpan.FromTicks(MasterBroadcastInterval.Ticks * MissingMasterCount); }
        }

        /// <summary>
        /// Computes and returns the maximum time a master instance should wait for
        /// a slave to broadcast a <see cref="ClusterMemberStatus" /> before concluding that
        /// the slave has gone offline.  This is computed by multiplying <see cref="SlaveUpdateInterval" />
        /// by <see cref="MissingSlaveCount" />.
        /// </summary>
        public TimeSpan MissingSlaveInterval
        {
            get { return TimeSpan.FromTicks(SlaveUpdateInterval.Ticks * MissingSlaveCount); }
        }

        /// <summary>
        /// Initializes an instance with reasonable default settings.
        /// </summary>
        /// <param name="clusterBaseEP">The cluster's logical base endpoint.</param>
        public ClusterMemberSettings(MsgEP clusterBaseEP)
        {
            if (!clusterBaseEP.IsLogical)
                throw new ArgumentException("Only logical cluster base endpoints are allowed.");

            this.ClusterBaseEP = clusterBaseEP;
        }

        /// <summary>
        /// Initializes an instance by loading settings from the application
        /// configuration using the specified configuration key prefix.
        /// </summary>
        /// <param name="keyPrefix">The fully qualified configuration key prefix.</param>
        /// <exception cref="ArgumentException">Thrown if the required <b>ClusterBaseEP</b> setting was not found.</exception>
        public ClusterMemberSettings(string keyPrefix)
            : this(keyPrefix, null)
        {
        }

        /// <summary>
        /// Initializes an instance by loading settings from the application
        /// configuration using the specified configuration key prefix.
        /// </summary>
        /// <param name="keyPrefix">The fully qualified configuration key prefix.</param>
        /// <param name="defClusterBaseEP">The default cluster <b>ClusterBaseEP</b> setting.</param>
        /// <exception cref="ArgumentException">Thrown if the required <b>ClusterBaseEP</b> setting was not found.</exception>
        public ClusterMemberSettings(string keyPrefix, MsgEP defClusterBaseEP)
        {
            var config = new Config(keyPrefix);

            this.ClusterBaseEP = config.Get("ClusterBaseEP", (string)defClusterBaseEP);
            if (this.ClusterBaseEP == null)
                throw new ArgumentException(string.Format("[{0}ClusterBaseEP] configuration setting is required.", config.KeyPrefix));

            this.Mode = config.Get<ClusterMemberMode>("Mode", ClusterMemberMode.Normal);
            if (this.Mode == ClusterMemberMode.Unknown)
                throw new ArgumentException(string.Format("[{0}Mode] cannot be [Unknown].", config.KeyPrefix));

            this.MasterBroadcastInterval = config.Get("MasterBroadcastInterval", this.MasterBroadcastInterval);
            this.SlaveUpdateInterval     = config.Get("SlaveUpdateInterval", this.SlaveUpdateInterval);
            this.ElectionInterval        = config.Get("ElectionInterval", this.ElectionInterval);
            this.MissingMasterCount      = config.Get("MissingMasterCount", this.MissingMasterCount);
            this.MissingSlaveCount       = config.Get("MissingSlaveCount", this.MissingSlaveCount);
            this.MasterBkInterval        = config.Get("MasterBkInterval", this.MasterBkInterval);
            this.SlaveBkInterval         = config.Get("SlaveBkInterval", this.SlaveBkInterval);
            this.BkInterval              = config.Get("BkInterval", this.BkInterval);
        }

        /// <summary>
        /// Used internally to load the cluster member settings from a <see cref="ClusterMemberStatus" /> instance.
        /// </summary>
        /// <param name="memberStatus">The member status.</param>
        internal ClusterMemberSettings(ClusterMemberStatus memberStatus)
        {
            this.ClusterBaseEP           = memberStatus["_s.ClusterBaseEP"];
            this.Mode                    = (ClusterMemberMode)Serialize.Parse(memberStatus["_s.Mode"], typeof(ClusterMemberMode), ClusterMemberMode.Unknown);
            this.MasterBroadcastInterval = Serialize.Parse(memberStatus["_s.MasterBroadcastInterval"], TimeSpan.Zero);
            this.SlaveUpdateInterval     = Serialize.Parse(memberStatus["_s.SlaveUpdateInterval"], TimeSpan.Zero);
            this.ElectionInterval        = Serialize.Parse(memberStatus["_s.ElectionInterval"], TimeSpan.Zero);
            this.MissingMasterCount      = Serialize.Parse(memberStatus["_s.MissingMasterCount"], 0);
            this.MissingSlaveCount       = Serialize.Parse(memberStatus["_s.MissingSlaveCount"], 0);
            this.MasterBkInterval        = Serialize.Parse(memberStatus["_s.MasterBkInterval"], TimeSpan.Zero);
            this.SlaveBkInterval         = Serialize.Parse(memberStatus["_s.SlaveBkInterval"], TimeSpan.Zero);
            this.BkInterval              = Serialize.Parse(memberStatus["_s.BkInterval"], TimeSpan.Zero);
        }

        /// <summary>
        /// Used internally to save the member settings to a <see cref="ClusterMemberStatus" /> instance.
        /// </summary>
        /// <param name="memberStatus">The member status.</param>
        internal void SaveTo(ClusterMemberStatus memberStatus)
        {
            memberStatus["_s.ClusterBaseEP"]           = this.ClusterBaseEP;
            memberStatus["_s.Mode"]                    = this.Mode.ToString();
            memberStatus["_s.MasterBroadcastInterval"] = Serialize.ToString(this.MasterBroadcastInterval);
            memberStatus["_s.SlaveUpdateInterval"]     = Serialize.ToString(this.SlaveUpdateInterval);
            memberStatus["_s.ElectionInterval"]        = Serialize.ToString(this.ElectionInterval);
            memberStatus["_s.MissingMasterCount"]      = Serialize.ToString(this.MissingMasterCount);
            memberStatus["_s.MissingSlaveCount"]       = Serialize.ToString(this.MissingSlaveCount);
            memberStatus["_s.MasterBkInterval"]        = Serialize.ToString(this.MasterBkInterval);
            memberStatus["_s.SlaveBkInterval"]         = Serialize.ToString(this.SlaveBkInterval);
            memberStatus["_s.BkInterval"]              = Serialize.ToString(this.BkInterval);
        }

        /// <summary>
        /// Returns <c>true</c> if the settings object passed equal these settings.
        /// </summary>
        /// <param name="obj">The object to be compared.</param>
        /// <returns><c>true</c> if the objects are equal.</returns>
        /// <remarks>
        /// <note>
        /// The <see cref="Mode" /> property is ignored when comparring
        /// two <see cref="ClusterMemberSettings" /> instances.
        /// </note>
        /// </remarks>
        public override bool Equals(object obj)
        {
            var settings = obj as ClusterMemberSettings;

            if (settings == null)
                return false;

            return this.ClusterBaseEP.Equals(settings.ClusterBaseEP) &&
                   this.MasterBroadcastInterval == settings.MasterBroadcastInterval &&
                   this.SlaveUpdateInterval == settings.SlaveUpdateInterval &&
                   this.ElectionInterval == settings.ElectionInterval &&
                   this.MissingMasterCount == settings.MissingMasterCount &&
                   this.MissingSlaveCount == settings.MissingSlaveCount &&
                   this.MasterBkInterval == settings.MasterBkInterval &&
                   this.SlaveBkInterval == settings.SlaveBkInterval &&
                   this.BkInterval == settings.BkInterval;
        }

        /// <summary>
        /// Overriding this to avoid the compiler warning.
        /// </summary>
        /// <returns>Returns zero.</returns>
        public override int GetHashCode()
        {
            return 0;
        }
    }
}
