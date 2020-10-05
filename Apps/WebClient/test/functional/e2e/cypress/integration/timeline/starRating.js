const { AuthMethod } = require("../../support/constants")

describe('Validate Star Rating', () => {
    beforeEach(() => {
    })

    it('Cliking the 5 star button should logout', () => {
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak)
        cy.get('[data-testid=logoutBtn]').click()
        cy.get('[data-testid=formRating] > .b-rating-star-empty:last').click()
        cy.url().should('include', '/logout')
    })

    it('Clicking Skip button should logout', () => {
        cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak)
        cy.get('[data-testid=logoutBtn]').click()
        cy.get('[data-testid=ratingModalSkipBtn]').click()
        cy.url().should('include', '/logout')
    })
})