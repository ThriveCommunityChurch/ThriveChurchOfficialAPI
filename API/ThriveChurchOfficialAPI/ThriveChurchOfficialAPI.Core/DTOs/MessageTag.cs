using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Core
{
    /// <summary>
    /// Categorizes sermon messages by topic/theme to enable better organization and discovery
    /// </summary>
    public enum MessageTag
    {
        /// <summary>
        /// Default tag type of unknown is not supported and only used for validation
        /// </summary>
        Unknown = -1,

        // Relationships & Family
        /// <summary>
        /// Topics related to marriage, dating, and romantic relationships
        /// </summary>
        Marriage,

        /// <summary>
        /// Topics related to parenting, children, and family dynamics
        /// </summary>
        Family,

        /// <summary>
        /// Topics related to friendships and community relationships
        /// </summary>
        Friendship,

        /// <summary>
        /// Topics related to singleness and navigating life as a single person
        /// </summary>
        Singleness,

        // Financial & Stewardship
        /// <summary>
        /// Topics related to money management, budgeting, and financial wisdom
        /// </summary>
        FinancialStewardship,

        /// <summary>
        /// Topics related to generosity, tithing, and giving
        /// </summary>
        Generosity,

        // Theological Foundations
        /// <summary>
        /// Topics related to the nature and character of God
        /// </summary>
        NatureOfGod,

        /// <summary>
        /// Topics related to the Trinity (Father, Son, Holy Spirit)
        /// </summary>
        Trinity,

        /// <summary>
        /// Topics related to salvation, grace, and redemption
        /// </summary>
        Salvation,

        /// <summary>
        /// Topics related to the resurrection of Jesus and believers
        /// </summary>
        Resurrection,

        /// <summary>
        /// Topics related to the Holy Spirit and spiritual gifts
        /// </summary>
        HolySpirit,

        /// <summary>
        /// Topics related to the church, ecclesiology, and the body of Christ
        /// </summary>
        Church,

        /// <summary>
        /// Topics related to end times, eschatology, and the return of Christ
        /// </summary>
        EndTimes,

        /// <summary>
        /// Topics related to sin, repentance, and forgiveness
        /// </summary>
        SinAndRepentance,

        /// <summary>
        /// Topics related to faith, belief, and trust in God
        /// </summary>
        Faith,

        /// <summary>
        /// Topics related to sanctification and becoming more Christ-like
        /// </summary>
        Sanctification,

        /// <summary>
        /// Topics related to biblical covenants (Abrahamic, Mosaic, Davidic, New Covenant)
        /// </summary>
        Covenant,

        /// <summary>
        /// Topics related to defending the Christian faith and answering objections
        /// </summary>
        Apologetics,

        // Spiritual Disciplines
        /// <summary>
        /// Topics related to prayer and intercession
        /// </summary>
        Prayer,

        /// <summary>
        /// Topics related to fasting and spiritual discipline
        /// </summary>
        Fasting,

        /// <summary>
        /// Topics related to worship and praise
        /// </summary>
        Worship,

        /// <summary>
        /// Topics related to Bible study and Scripture engagement
        /// </summary>
        BibleStudy,

        /// <summary>
        /// Topics related to meditation and contemplation
        /// </summary>
        Meditation,

        /// <summary>
        /// Topics related to service and ministry to others
        /// </summary>
        Service,

        /// <summary>
        /// Topics related to praise and praising God
        /// </summary>
        Praise,

        // Sacraments & Ordinances
        /// <summary>
        /// Topics related to baptism and its significance
        /// </summary>
        Baptism,

        /// <summary>
        /// Topics related to communion, the Lord's Supper, or Eucharist
        /// </summary>
        Communion,

        // Life Stages & Transitions
        /// <summary>
        /// Topics related to youth and young adult life
        /// </summary>
        Youth,

        /// <summary>
        /// Topics related to aging, retirement, and senior life
        /// </summary>
        Aging,

        /// <summary>
        /// Topics related to grief, loss, and mourning
        /// </summary>
        GriefAndLoss,

        /// <summary>
        /// Topics related to major life transitions and changes
        /// </summary>
        LifeTransitions,

        // Social Issues & Justice
        /// <summary>
        /// Topics related to social justice, equity, and righteousness
        /// </summary>
        SocialJustice,

        /// <summary>
        /// Topics related to racial reconciliation and unity
        /// </summary>
        RacialReconciliation,

        /// <summary>
        /// Topics related to poverty, homelessness, and economic justice
        /// </summary>
        Poverty,

        /// <summary>
        /// Topics related to caring for creation and environmental stewardship
        /// </summary>
        Creation,

        /// <summary>
        /// Topics related to politics, government, and civic engagement
        /// </summary>
        Politics,

        // Personal Growth & Character
        /// <summary>
        /// Topics related to identity in Christ and self-worth
        /// </summary>
        Identity,

        /// <summary>
        /// Topics related to purpose, calling, and vocation
        /// </summary>
        Purpose,

        /// <summary>
        /// Topics related to courage, bravery, and overcoming fear
        /// </summary>
        Courage,

        /// <summary>
        /// Topics related to hope and optimism
        /// </summary>
        Hope,

        /// <summary>
        /// Topics related to love and compassion
        /// </summary>
        Love,

        /// <summary>
        /// Topics related to joy and contentment
        /// </summary>
        Joy,

        /// <summary>
        /// Topics related to peace and rest
        /// </summary>
        Peace,

        /// <summary>
        /// Topics related to patience and perseverance
        /// </summary>
        Patience,

        /// <summary>
        /// Topics related to humility and servanthood
        /// </summary>
        Humility,

        /// <summary>
        /// Topics related to wisdom and discernment
        /// </summary>
        Wisdom,

        /// <summary>
        /// Topics related to integrity and character
        /// </summary>
        Integrity,

        /// <summary>
        /// Topics related to forgiveness and reconciliation
        /// </summary>
        Forgiveness,

        /// <summary>
        /// Topics related to gratitude and thankfulness
        /// </summary>
        Gratitude,

        /// <summary>
        /// Topics related to trust and trusting God in all circumstances
        /// </summary>
        Trust,

        /// <summary>
        /// Topics related to obedience and following God's commands
        /// </summary>
        Obedience,

        /// <summary>
        /// Topics related to contentment and finding satisfaction in God
        /// </summary>
        Contentment,

        /// <summary>
        /// Topics related to pride and dealing with arrogance
        /// </summary>
        Pride,

        /// <summary>
        /// Topics related to fear and overcoming fear with faith
        /// </summary>
        Fear,

        /// <summary>
        /// Topics related to anger and managing it biblically
        /// </summary>
        Anger,

        // Challenges & Struggles
        /// <summary>
        /// Topics related to suffering, trials, and hardship
        /// </summary>
        Suffering,

        /// <summary>
        /// Topics related to doubt and questions of faith
        /// </summary>
        Doubt,

        /// <summary>
        /// Topics related to anxiety, worry, and mental health
        /// </summary>
        Anxiety,

        /// <summary>
        /// Topics related to depression and emotional struggles
        /// </summary>
        Depression,

        /// <summary>
        /// Topics related to addiction and recovery
        /// </summary>
        Addiction,

        /// <summary>
        /// Topics related to temptation and spiritual warfare
        /// </summary>
        Temptation,

        /// <summary>
        /// Topics related to spiritual warfare and battling spiritual forces
        /// </summary>
        SpiritualWarfare,

        /// <summary>
        /// Topics related to persecution and enduring hardship for faith
        /// </summary>
        Persecution,

        // Eternal & Supernatural
        /// <summary>
        /// Topics related to heaven and eternal life
        /// </summary>
        Heaven,

        /// <summary>
        /// Topics related to hell and eternal judgment
        /// </summary>
        Hell,

        // Mission & Evangelism
        /// <summary>
        /// Topics related to evangelism and sharing faith
        /// </summary>
        Evangelism,

        /// <summary>
        /// Topics related to missions and global outreach
        /// </summary>
        Missions,

        /// <summary>
        /// Topics related to discipleship and spiritual growth
        /// </summary>
        Discipleship,

        /// <summary>
        /// Topics related to leadership and influence
        /// </summary>
        Leadership,

        /// <summary>
        /// Topics related to personal evangelism, witnessing, and sharing testimony
        /// </summary>
        Witnessing,

        // Biblical Studies
        /// <summary>
        /// Topics related to the parables of Jesus
        /// </summary>
        Parables,

        /// <summary>
        /// Topics related to the Sermon on the Mount (Matthew 5-7)
        /// </summary>
        SermonOnTheMount,

        /// <summary>
        /// Topics related to the Fruit of the Spirit (Galatians 5:22-23)
        /// </summary>
        FruitOfTheSpirit,

        /// <summary>
        /// Topics related to the Armor of God (Ephesians 6)
        /// </summary>
        ArmorOfGod,

        /// <summary>
        /// Topics related to Old Testament prophets and their messages
        /// </summary>
        Prophets,

        // Biblical Book Studies
        /// <summary>
        /// Sermon series studying the book of Genesis
        /// </summary>
        Genesis,

        /// <summary>
        /// Sermon series studying the book of Exodus
        /// </summary>
        Exodus,

        /// <summary>
        /// Sermon series studying the book of Psalms
        /// </summary>
        Psalms,

        /// <summary>
        /// Sermon series studying the book of Proverbs
        /// </summary>
        Proverbs,

        /// <summary>
        /// Sermon series studying the Gospels (Matthew, Mark, Luke, John)
        /// </summary>
        Gospels,

        /// <summary>
        /// Sermon series studying the book of Acts
        /// </summary>
        Acts,

        /// <summary>
        /// Sermon series studying the book of Romans
        /// </summary>
        Romans,

        /// <summary>
        /// Sermon series studying other Pauline epistles
        /// </summary>
        PaulineEpistles,

        /// <summary>
        /// Sermon series studying Revelation
        /// </summary>
        Revelation,

        /// <summary>
        /// General Old Testament book studies not otherwise categorized
        /// </summary>
        OldTestament,

        /// <summary>
        /// General New Testament book studies not otherwise categorized
        /// </summary>
        NewTestament,

        // Seasonal & Liturgical
        /// <summary>
        /// Topics related to Advent season
        /// </summary>
        Advent,

        /// <summary>
        /// Topics related to Christmas season
        /// </summary>
        Christmas,

        /// <summary>
        /// Topics related to Lent season
        /// </summary>
        Lent,

        /// <summary>
        /// Topics related to Easter season
        /// </summary>
        Easter,

        /// <summary>
        /// Topics related to Pentecost
        /// </summary>
        Pentecost,

        // Work & Vocation
        /// <summary>
        /// Topics related to work, career, and professional life
        /// </summary>
        Work,

        /// <summary>
        /// Topics related to rest, sabbath, and work-life balance
        /// </summary>
        Rest,

        // Gender & Relationships
        /// <summary>
        /// Topics related to biblical manhood and what it means to be a godly man
        /// </summary>
        BiblicalManhood,

        /// <summary>
        /// Topics related to biblical womanhood and what it means to be a godly woman
        /// </summary>
        BiblicalWomanhood,

        /// <summary>
        /// Topics related to sexual purity and biblical sexuality
        /// </summary>
        SexualPurity,

        // Other
        /// <summary>
        /// Topics related to miracles and the supernatural
        /// </summary>
        Miracles,

        /// <summary>
        /// Topics related to prophecy and prophetic ministry
        /// </summary>
        Prophecy,

        /// <summary>
        /// Topics related to healing and restoration
        /// </summary>
        Healing,

        /// <summary>
        /// Topics related to community and fellowship
        /// </summary>
        Community,

        /// <summary>
        /// Topics related to culture and cultural engagement
        /// </summary>
        Culture,

        /// <summary>
        /// Topics related to technology and modern life
        /// </summary>
        Technology
    }
}

