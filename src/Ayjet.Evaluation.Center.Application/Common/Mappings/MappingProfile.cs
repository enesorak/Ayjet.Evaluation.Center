using AutoMapper;
using Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetCounts;
using Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetList;
using Ayjet.Evaluation.Center.Application.Features.MultipleChoiceQuestions.Queries.GetListByTestDefinition;
using Ayjet.Evaluation.Center.Application.Features.PsychometricQuestions.Queries.GetById;
using Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetList;
using Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetTestProgress;
using Ayjet.Evaluation.Center.Application.Features.TestDefinitions.Queries.GetList;
using Ayjet.Evaluation.Center.Application.Features.TestResults.Queries.GetByAssignmentId;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Models;

namespace Ayjet.Evaluation.Center.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TestDefinition, TestDefinitionDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

        CreateMap<AnswerOption, QuestionAnswerOptionDto>()
            .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Translations.FirstOrDefault().Text));


        CreateMap<TestResult, TestResultDto>()
            .ForMember(dest => dest.AssignmentId, opt => opt.MapFrom(src => src.TestAssignmentId))
            .ForMember(dest => dest.Candidate, opt => opt.MapFrom(src => src.TestAssignment.Candidate)) // <-- Değişti
            .ForMember(dest => dest.TestTitle, opt => opt.MapFrom(src => src.TestAssignment.TestDefinition.Title))
            .ForMember(dest => dest.StartedAt, opt => opt.MapFrom(src => src.TestAssignment.StartedAt))
            .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.TestAssignment.CompletedAt))
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.ResultsPayloadJson))
            .ForMember(dest => dest.TestType,
                opt => opt.MapFrom(src => src.TestAssignment.TestDefinition.Type.ToString()));


        CreateMap<TestAssignment, TestAssignmentDto>()
            .ForMember(dest => dest.AssignmentId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CandidateFullName, opt => opt.MapFrom(src => src.Candidate.FullName))
            .ForMember(dest => dest.TestTitle, opt => opt.MapFrom(src => src.TestDefinition.Title))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<Candidate, CandidateDto>()
            .ForMember(dest => dest.CandidateType, opt => opt.MapFrom(src => src.CandidateType.ToString()))
            .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department.ToString()))
            .ForMember(dest => dest.Gender,
                opt => opt.MapFrom(src => src.Gender)); // Gender zaten nullable enum, direkt map'lenebilir.

        CreateMap<CandidateCounts, CandidateCountsDto>();
        
        
        CreateMap<TestAssignment, TestProgressDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString())) // <-- YENİ KURAL
            .ForMember(dest => dest.Candidate, opt => opt.MapFrom(src => src.Candidate))
 

            .ForMember(dest => dest.TestTitle, opt => opt.MapFrom(src => src.TestDefinition.Title))
            .ForMember(dest => dest.TestType, opt => opt.MapFrom(src => src.TestDefinition.Type))
            .ForMember(dest => dest.TimeLimitInMinutes, opt => opt.MapFrom(src => src.TimeLimitInMinutes))
            .ForMember(dest => dest.StartedAt, opt => opt.MapFrom(src => src.StartedAt.HasValue ? src.StartedAt.Value.ToString("o") : null));



        CreateMap<PsychometricQuestion, PsychometricQuestionDetailDto>();
        CreateMap<PsychometricQuestionTranslation, TranslationDto>();
        CreateMap<QuestionScaleMapping, ScaleMappingDto>()
            .ForMember(dest => dest.ScaleName, opt => opt.MapFrom(src => src.PsychometricScale.Name));
    }
}