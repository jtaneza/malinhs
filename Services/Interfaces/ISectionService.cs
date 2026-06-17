using MalikongkongNHS.Models.Entities;

namespace MalikongkongNHS.Services.Interfaces;

public interface ISectionService
{
    List<Section> GetAll();

    Section? GetById(int id);

    void Add(Section section);

    void Update(Section section);

    void Delete(int id);
}