const { AuthMethod } = require("../../../support/constants");

describe("Validate Star Rating", () => {
    beforeEach(() => {
        cy.enableModules(["Medication", "Note"]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Clicking the 5 star button should log out", () => {
        cy.get("[data-testid=headerDropdownBtn]").click();
        cy.get("[data-testid=logoutBtn]").click();
        cy.get("[data-testid=formRating] > .b-rating-star-empty:last").click();
        cy.url().should("include", "/logout");
    });

    it("Clicking Skip button should log out", () => {
        cy.get("[data-testid=headerDropdownBtn]").click();
        cy.get("[data-testid=logoutBtn]").click();
        cy.get("[data-testid=ratingModalSkipBtn]").click();
        cy.url().should("include", "/logout");
    });
});
