<template>
  <div id="app">
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
      <router-link class="navbar-brand" to="/">Movie Bot</router-link>
      <button
        class="navbar-toggler"
        type="button"
        data-toggle="collapse"
        data-target="#navbarColor02"
        aria-controls="navbarColor02"
        aria-expanded="false"
        aria-label="Toggle navigation"
      >
        <span class="navbar-toggler-icon"></span>
      </button>

      <div class="collapse navbar-collapse" id="navbarColor02">
        <ul class="navbar-nav mr-auto">
          <router-link to="/" v-slot="{ href, route, navigate, isExactActive }">
            <li :class="[isExactActive && 'active', 'nav-item']">
              <a :href="href" @click="navigate" class="nav-link">
                Home <span class="sr-only">(current)</span>
              </a>
            </li>
          </router-link>

          <router-link
            to="/about"
            v-slot="{ href, route, navigate, isExactActive }"
          >
            <li :class="[isExactActive && 'active', 'nav-item']">
              <a :href="href" @click="navigate" class="nav-link"> About</a>
            </li>
          </router-link>
        </ul>
      </div>
    </nav>
    <router-view />
  </div>
</template>

<style lang="scss">
</style>
