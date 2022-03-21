﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SimilarCode.Load;

#nullable disable

namespace SimilarCode.Load.Migrations
{
    [DbContext(typeof(AnswersContext))]
    [Migration("20220320232301_NewMigration")]
    partial class NewMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.3");

            modelBuilder.Entity("SimilarCode.Load.Models.Answer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Answers");
                });

            modelBuilder.Entity("SimilarCode.Load.Models.CodeSnippet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CodeSnippetGroupingId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CodeSnippetGroupingId");

                    b.ToTable("CodeSnippets");
                });

            modelBuilder.Entity("SimilarCode.Load.Models.CodeSnippetGrouping", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AnswerId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("AnswerId");

                    b.ToTable("CodeSnippetGroupings");
                });

            modelBuilder.Entity("SimilarCode.Load.Models.ProgrammingLanguage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("CodeSnippetId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Language")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CodeSnippetId");

                    b.ToTable("ProgrammingLanguage");
                });

            modelBuilder.Entity("SimilarCode.Load.Models.CodeSnippet", b =>
                {
                    b.HasOne("SimilarCode.Load.Models.CodeSnippetGrouping", "CodeSnippetGrouping")
                        .WithMany("CodeSnippets")
                        .HasForeignKey("CodeSnippetGroupingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CodeSnippetGrouping");
                });

            modelBuilder.Entity("SimilarCode.Load.Models.CodeSnippetGrouping", b =>
                {
                    b.HasOne("SimilarCode.Load.Models.Answer", "Answer")
                        .WithMany("CodeSnippetGroups")
                        .HasForeignKey("AnswerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Answer");
                });

            modelBuilder.Entity("SimilarCode.Load.Models.ProgrammingLanguage", b =>
                {
                    b.HasOne("SimilarCode.Load.Models.CodeSnippet", null)
                        .WithMany("ProgrammingLanguage")
                        .HasForeignKey("CodeSnippetId");
                });

            modelBuilder.Entity("SimilarCode.Load.Models.Answer", b =>
                {
                    b.Navigation("CodeSnippetGroups");
                });

            modelBuilder.Entity("SimilarCode.Load.Models.CodeSnippet", b =>
                {
                    b.Navigation("ProgrammingLanguage");
                });

            modelBuilder.Entity("SimilarCode.Load.Models.CodeSnippetGrouping", b =>
                {
                    b.Navigation("CodeSnippets");
                });
#pragma warning restore 612, 618
        }
    }
}
