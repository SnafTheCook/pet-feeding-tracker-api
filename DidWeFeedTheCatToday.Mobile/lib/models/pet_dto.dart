class PetDto {
  final int id;
  final String name;
  final DateTime? lastFed;
  final String? additionalInformation;

  PetDto({
    required this.id,
    required this.name,
    this.lastFed,
    this.additionalInformation
  });

  factory PetDto.fromJson(Map<String, dynamic> json)
  {
    return PetDto(
      id: json['id'] as int,
      name: json['name'] as String,
      additionalInformation: json['additionalInformation'] != null 
      ? json['additionalInformation']as String
      : null,
      lastFed: json['lastFed'] != null 
      ? DateTime.parse(json['lastFed']) 
      : null
    );
  }
  
}