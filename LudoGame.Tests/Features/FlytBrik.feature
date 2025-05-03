Feature: Flyt brik frem på banen
  For at kunne flytte brikker i Ludo-spillet
  Som spiller
  Skal jeg kunne flytte mine brikker baseret på terningeslag

  Scenario: Flyt brik ud fra hjem med en 6'er
    Given en spiller har en brik i hjemmet
    When spilleren slår en 6'er
    Then brikken flyttes ud på startfeltet

  Scenario: Kan ikke flytte brik ud fra hjem uden en 6'er
    Given en spiller har en brik i hjemmet
    When spilleren slår en 4'er
    Then brikken forbliver i hjemmet

  Scenario: Flyt brik frem på banen
    Given en spiller har en brik på felt 0
    When spilleren slår en 3'er
    Then brikken flyttes til felt 3

  Scenario: Slå en modstanders brik hjem
    Given en spiller har en brik på felt 5
    And en modstander har en brik på felt 8
    When spilleren slår en 3'er
    Then modstanderens brik flyttes hjem
    And spillerens brik flyttes til felt 8
