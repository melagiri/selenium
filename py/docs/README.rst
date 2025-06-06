==========================
About Python Documentation
==========================

This directory, ``py/docs``, is the source for the API Reference Documentation
and basic Python documentation as well as the main README for the GitHub and
PyPI package.


How to build docs
=================

One can build the Python documentation without doing a full bazel build. The
following instructions will build the setup a virtual environment and installs tox,
clones the selenium repo and then runs ``tox -c py/tox.ini -e docs``, building the
Python documentation.

.. code-block:: console

    python3 -m venv venv
    source venv/bin/activate
    pip install tox
    git clone git@github.com:SeleniumHQ/selenium.git
    cd selenium/
    # uncomment and switch to your development branch if needed 
    #git switch -c feature-branch origin/feature-branch
    tox -c py/tox.ini -e docs

This works in a similar manner as the larger selenium bazel build, but does just the Python
documentation portion.


What is happening under the covers of tox, sphinx
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

tox is essentially a build tool for Python. Here it sets up its own virtual env and installs
the documentation packages (sphinx and jinja2) as well as the required selenium python
dependencies. Then tox runs the ``sphinx-autogen`` command to generate autodoc stub pages.
Then it runs ``sphinx-build`` to build the HTML docs.

Sphinx is .. well a much larger topic then what we could cover here. Most important to say
here is that the docs are using the "sphinx-material" theme (AKA "Material for Sphinx" theme)
and the the majority of the api documentation is autogenerated. There is plenty of information
available and currently the documentation is fairly small and not complex. So some basic
understanding of reStructuredText and the Sphinx tool chain should be sufficient to contribute
and develop the Python docs.


To clean up the build assets and tox cache
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Although there is a Sphinx Makefile option, to clean up using the tox environment above, one can
manually clean the build assets by deleting the ``build`` directory on the root (``selenium/build``
using the default directory on git clone). Note that tox caches parts of the build and recognizes
changes to speed up the build. To start fresh, delete the aformentioned directory to clean the
Sphinx build assets and delete the ``selenium/py/.tox`` directory to clean the tox environment.


Known documentation issues
==========================

The API Reference primarily builds from the source code. But currently the initial template stating
which modules to document is hard coded within ``py/docs/source/api.rst``. So if modules are added or
removed, then the generated docs will be inaccurate. It would be preferred that the API docs generate
soley from the code if possible. This is being tracked in
`#14178 <https://github.com/SeleniumHQ/selenium/issues/14178>`_

We are working through the Sphinx build warnings and errors, trying to clean up both the syntax and
the build.


Contributing to Python docs
===========================

First it is recommended that you read the main `CONTRIBUTING.md <https://github.com/SeleniumHQ/selenium/blob/trunk/CONTRIBUTING.md>`_.

Some steps for contibuting to the Python documentation ...

- Check out changes locally using instructions above.
- Try to resolve any warnings/errors.
  - If too arduous, either ask for help or add to the list of known issues.
- If this process is updated, please update this doc as well to help the next person.
