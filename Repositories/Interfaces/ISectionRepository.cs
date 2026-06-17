using MalikongkongNHS.Models.Entities;

namespace MalikongkongNHS.Repositories.Interfaces;

public interface ISectionRepository
{
    List<Section> GetAll();

    Section? GetById(int id);

    void Add(Section section);

    void Update(Section section);

    void Delete(int id);
}