using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Conductor.Domain.Entities
{
    /*
    SET ANSI_NULLS ON
	GO

	SET QUOTED_IDENTIFIER ON
	GO

	CREATE TABLE [dbo].[FlowDefinition](
		[FlowId] [UNIQUEIDENTIFIER] NOT NULL,
		[FlowName] [NVARCHAR](100) NOT NULL,
		[Definition] [NVARCHAR](MAX) NOT NULL,
		[DefinitionId] [NVARCHAR](100) NOT NULL,
		[DefinitionVersion] [INT] NOT NULL,
		[Description] [NVARCHAR](500) NULL,
		[EntryPoint] [NVARCHAR](500) NULL,
		[EntryPointPath] [NVARCHAR](100) NULL,
		[CreateTime] [DATETIME] NOT NULL,
		[Creater] [NVARCHAR](50) NULL,
	 CONSTRAINT [PK_FlowDefinition] PRIMARY KEY CLUSTERED 
	(
		[FlowId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
	GO
     */

    /// <summary>
    /// 流程定义
    /// </summary>
    [Table("FlowDefinition")]
    public class FlowDefinition
    {
        /// <summary>
        /// 流程 ID
        /// </summary>
        [Key]
        [Required]
        public Guid FlowId { get; set; }

        /// <summary>
        /// 流程名称
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string FlowName { get; set; }

        /// <summary>
        /// 工作流定义（JSON字符串）
        /// </summary>
        [Required]
        public string Definition { get; set; }

        /// <summary>
        /// 定义 id
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string DefinitionId { get; set; }

        /// <summary>
        /// 定义版本
        /// </summary>
        [Required]
        public int DefinitionVersion { get; set; }

        /// <summary>
        /// 流程描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 流程入口点 (JSON字符串)
        /// </summary>
        [MaxLength(500)]
        public string EntryPoint { get; set; }

        /// <summary>
        /// 流程入口点路径
        /// </summary>
        [MaxLength(100)]
        public string EntryPointPath { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Required]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string Creator { get; set; }
    }
}